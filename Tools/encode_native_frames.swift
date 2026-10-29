import Foundation
import ImageIO
import UniformTypeIdentifiers
import AVFoundation
import CoreVideo

// Encode native Unity review frames as an animation without modifying the PNGs.
let arguments = CommandLine.arguments
guard arguments.count >= 3 else { fatalError("Usage: encode_native_frames.swift FRAMES OUTPUT.gif [FPS]") }
let source = URL(fileURLWithPath: arguments[1], isDirectory: true)
let output = URL(fileURLWithPath: arguments[2])
let fps = arguments.count > 3 ? Double(arguments[3]) ?? 24 : 24
let frames = try FileManager.default.contentsOfDirectory(at: source, includingPropertiesForKeys: nil)
    .filter { $0.pathExtension.lowercased() == "png" }.sorted { $0.lastPathComponent < $1.lastPathComponent }
guard !frames.isEmpty, fps > 0 else { fatalError("No native frames or invalid frame rate") }
if output.pathExtension.lowercased() == "mp4" {
    guard let first = CGImageSourceCreateWithURL(frames[0] as CFURL, nil), let image = CGImageSourceCreateImageAtIndex(first, 0, nil) else { fatalError("Cannot inspect first frame") }
    if FileManager.default.fileExists(atPath: output.path) { try FileManager.default.removeItem(at: output) }
    let writer = try AVAssetWriter(outputURL: output, fileType: .mp4)
    let input = AVAssetWriterInput(mediaType: .video, outputSettings: [AVVideoCodecKey: AVVideoCodecType.h264, AVVideoWidthKey: image.width, AVVideoHeightKey: image.height, AVVideoCompressionPropertiesKey: [AVVideoAverageBitRateKey: 5_000_000, AVVideoMaxKeyFrameIntervalKey: Int(fps)]])
    input.expectsMediaDataInRealTime = false
    let adaptor = AVAssetWriterInputPixelBufferAdaptor(assetWriterInput: input, sourcePixelBufferAttributes: [kCVPixelBufferPixelFormatTypeKey as String: kCVPixelFormatType_32ARGB, kCVPixelBufferWidthKey as String: image.width, kCVPixelBufferHeightKey as String: image.height, kCVPixelBufferCGBitmapContextCompatibilityKey as String: true])
    writer.add(input); guard writer.startWriting() else { fatalError("Cannot start movie: \(String(describing: writer.error))") }; writer.startSession(atSourceTime: .zero)
    for (index, frame) in frames.enumerated() {
        while !input.isReadyForMoreMediaData { if writer.status == .failed { fatalError("Movie failed: \(String(describing: writer.error))") }; usleep(1000) }
        guard let src = CGImageSourceCreateWithURL(frame as CFURL, nil), let picture = CGImageSourceCreateImageAtIndex(src, 0, nil), let pool = adaptor.pixelBufferPool else { fatalError("Cannot decode native frame") }
        var buffer: CVPixelBuffer?; guard CVPixelBufferPoolCreatePixelBuffer(nil, pool, &buffer) == kCVReturnSuccess, let pixel = buffer else { fatalError("Cannot allocate encoder buffer") }
        CVPixelBufferLockBaseAddress(pixel, [])
        guard let context = CGContext(data: CVPixelBufferGetBaseAddress(pixel), width: image.width, height: image.height, bitsPerComponent: 8, bytesPerRow: CVPixelBufferGetBytesPerRow(pixel), space: CGColorSpaceCreateDeviceRGB(), bitmapInfo: CGImageAlphaInfo.noneSkipFirst.rawValue) else { fatalError("Cannot create encoder context") }
        context.draw(picture, in: CGRect(x: 0, y: 0, width: image.width, height: image.height))
        CVPixelBufferUnlockBaseAddress(pixel, [])
        guard adaptor.append(pixel, withPresentationTime: CMTime(value: Int64(index), timescale: Int32(fps))) else { fatalError("Cannot append native frame: \(String(describing: writer.error))") }
    }
    input.markAsFinished(); let finished = DispatchSemaphore(value: 0); writer.finishWriting { finished.signal() }; finished.wait()
    guard writer.status == .completed else { fatalError("Movie finalization failed: \(String(describing: writer.error))") }
    let decoder = AVAssetImageGenerator(asset: AVURLAsset(url: output)); decoder.appliesPreferredTrackTransform = true
    let decoded = try decoder.copyCGImage(at: .zero, actualTime: nil)
    let checkURL = output.deletingPathExtension().appendingPathExtension("decoded-first.png")
    if let check = CGImageDestinationCreateWithURL(checkURL as CFURL, UTType.png.identifier as CFString, 1, nil) { CGImageDestinationAddImage(check, decoded, nil); CGImageDestinationFinalize(check) }
    print("Encoded \(frames.count) native frames as H.264 at \(fps) fps: \(output.path)"); exit(0)
}
guard let destination = CGImageDestinationCreateWithURL(output as CFURL, UTType.gif.identifier as CFString, frames.count, nil) else { fatalError("Cannot create animation") }
CGImageDestinationSetProperties(destination, [kCGImagePropertyGIFDictionary: [kCGImagePropertyGIFLoopCount: 0]] as CFDictionary)
for (index, frame) in frames.enumerated() {
    let duration = max(0.01, ((Double(index + 1) * 100 / fps).rounded() - (Double(index) * 100 / fps).rounded()) / 100)
    let properties = [kCGImagePropertyGIFDictionary: [kCGImagePropertyGIFDelayTime: duration, kCGImagePropertyGIFUnclampedDelayTime: duration]] as CFDictionary
    guard let image = CGImageSourceCreateWithURL(frame as CFURL, nil) else { fatalError("Unreadable native frame: \(frame.path)") }
    CGImageDestinationAddImageFromSource(destination, image, 0, properties)
}
guard CGImageDestinationFinalize(destination) else { fatalError("Animation encoding failed") }
print("Encoded \(frames.count) native frames at \(fps) fps: \(output.path)")
