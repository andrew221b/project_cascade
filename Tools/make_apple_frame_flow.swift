import Foundation
import Vision
import CoreGraphics
import ImageIO
import CoreVideo

let args=CommandLine.arguments
let source=URL(fileURLWithPath:args[1]);let destination=URL(fileURLWithPath:args[2])
let image=CGImageSourceCreateImageAtIndex(CGImageSourceCreateWithURL(source as CFURL,nil)!,0,nil)!
let side=image.width/4;precondition(image.height/3==side)
let resolution=96
func frame(_ index:Int)->CGImage {
 let crop=image.cropping(to:CGRect(x:(index%4)*side,y:(index/4)*side,width:side,height:side))!
 let ctx=CGContext(data:nil,width:resolution,height:resolution,bitsPerComponent:8,bytesPerRow:resolution*4,space:CGColorSpaceCreateDeviceRGB(),bitmapInfo:CGImageAlphaInfo.noneSkipLast.rawValue)!
 ctx.setFillColor(CGColor(gray:0.65,alpha:1));ctx.fill(CGRect(x:0,y:0,width:resolution,height:resolution))
 ctx.interpolationQuality = .high;ctx.draw(crop,in:CGRect(x:0,y:0,width:resolution,height:resolution));return ctx.makeImage()!
}
let images=(0..<12).map{frame($0)}
func flow(_ first:CGImage,_ next:CGImage)throws->[SIMD2<Float>] {
 let request=VNGenerateOpticalFlowRequest(targetedCGImage:next,options:[:]);request.revision=VNGenerateOpticalFlowRequestRevision1;request.computationAccuracy = .veryHigh
 request.outputPixelFormat = kCVPixelFormatType_TwoComponent32Float
 try VNImageRequestHandler(cgImage:first,options:[:]).perform([request])
 let pb=request.results!.first!.pixelBuffer;CVPixelBufferLockBaseAddress(pb,.readOnly);defer{CVPixelBufferUnlockBaseAddress(pb,.readOnly)}
 let pointer=CVPixelBufferGetBaseAddress(pb)!.assumingMemoryBound(to:Float.self),stride=CVPixelBufferGetBytesPerRow(pb)/4
 let width=CVPixelBufferGetWidth(pb),height=CVPixelBufferGetHeight(pb)
 var result=[SIMD2<Float>](repeating:.zero,count:resolution*resolution)
 func sample(_ x:Int,_ y:Int)->SIMD2<Float>{let n=max(0,min(height-1,y))*stride+max(0,min(width-1,x))*2;return SIMD2(pointer[n],pointer[n+1])}
 for y in 0..<resolution {for x in 0..<resolution {
  let xx=(Float(x)+0.5)*Float(width)/Float(resolution)-0.5, yy=(Float(y)+0.5)*Float(height)/Float(resolution)-0.5
  let ix=Int(floor(xx)),iy=Int(floor(yy)),fx=xx-Float(ix),fy=yy-Float(iy)
  let a=sample(ix,iy)*(1-fx)+sample(ix+1,iy)*fx,b=sample(ix,iy+1)*(1-fx)+sample(ix+1,iy+1)*fx
  result[y*resolution+x]=(a*(1-fy)+b*fy)*SIMD2(Float(resolution)/Float(width),Float(resolution)/Float(height))
 }}
 return result
}
// Guard vector direction and pixel units with a known translation.
// Revision 1 preserves thin hand/cloth edges more accurately on these drawings.
let control=CGContext(data:nil,width:resolution,height:resolution,bitsPerComponent:8,bytesPerRow:resolution*4,space:CGColorSpaceCreateDeviceRGB(),bitmapInfo:CGImageAlphaInfo.noneSkipLast.rawValue)!
control.setFillColor(CGColor(gray:0.65,alpha:1));control.fill(CGRect(x:0,y:0,width:resolution,height:resolution));control.draw(images[0],in:CGRect(x:8,y:8,width:resolution,height:resolution))
let measured=try flow(images[0],control.makeImage()!)[50*resolution+50]
precondition(abs(measured.x-8)<0.8 && abs(measured.y+8)<0.8,"Optical-flow direction/scale calibration failed")
print("Known displacement +8/-8 pixels, measured \(measured)")
// RG=forward A->B, BA=backward B->A. Quantized signed vectors in tile UV,
// +/- .25 covers 90 source pixels. Output rows are bottom-up for Unity.
let width=resolution*4,height=resolution*3
var packed=[UInt8](repeating:128,count:width*height*4)
for i in 0..<11 {
 let f=try flow(images[i],images[i+1]),b=try flow(images[i+1],images[i])
 for y in 0..<resolution { for x in 0..<resolution {
  let index=(((2-i/4)*resolution+(resolution-1-y))*width+(i%4)*resolution+x)*4
  let n=y*resolution+x
  for (j,value) in [f[n].x,-f[n].y,b[n].x,-b[n].y].enumerated(){packed[index+j]=UInt8(clamping:Int((value/Float(resolution)/0.5*255+128).rounded()))}
 }}
 print("Computed authored transition \(i) -> \(i+1)")
}
try Data(packed).write(to:destination);print("\(width)x\(height) RGBA flow, \(packed.count) bytes")
