import AppKit
import CoreText

// Layout renderer: native simulator screens are inserted intact; no generated UI.
let root = URL(fileURLWithPath: FileManager.default.currentDirectoryPath)
let assets = root.appendingPathComponent("Reports/StoreAssets-Oct09")
let fm = FileManager.default
for file in ["Alegreya-ExtraBold.ttf", "Nunito-SemiBold.ttf"] {
    CTFontManagerRegisterFontsForURL(root.appendingPathComponent("CascadeUnity/Assets/_Cascade/Art/Fonts/" + file) as CFURL, .process, nil)
}
func image(_ path: String) -> NSImage {
    guard let i = NSImage(contentsOf: assets.appendingPathComponent(path)) else { fatalError("Missing art: " + path) }
    return i
}
func color(_ hex: UInt32) -> NSColor {
    NSColor(srgbRed: CGFloat((hex >> 16) & 255)/255, green: CGFloat((hex >> 8) & 255)/255, blue: CGFloat(hex & 255)/255, alpha: 1)
}
func rect(_ x: CGFloat, _ top: CGFloat, _ w: CGFloat, _ h: CGFloat, _ height: CGFloat) -> NSRect {
    NSRect(x: x, y: height - top - h, width: w, height: h)
}
func canvas(_ width: Int, _ height: Int, _ path: String, alpha: Bool = false, draw: (CGFloat,CGFloat)->Void) {
    let space = CGColorSpace(name: CGColorSpace.sRGB)!
    let kind = alpha ? CGImageAlphaInfo.premultipliedLast : CGImageAlphaInfo.noneSkipLast
    guard let c = CGContext(data:nil,width:width,height:height,bitsPerComponent:8,bytesPerRow:width*4,space:space,bitmapInfo:kind.rawValue) else { fatalError("Canvas") }
    c.interpolationQuality = .high
    NSGraphicsContext.saveGraphicsState()
    NSGraphicsContext.current = NSGraphicsContext(cgContext:c,flipped:false)
    if !alpha { color(0x173B42).setFill(); NSRect(x:0,y:0,width:width,height:height).fill() }
    draw(CGFloat(width),CGFloat(height))
    let bitmap = NSBitmapImageRep(cgImage:c.makeImage()!)
    let output = assets.appendingPathComponent(path)
    try! fm.createDirectory(at:output.deletingLastPathComponent(),withIntermediateDirectories:true)
    try! bitmap.representation(using:.png,properties:[:])!.write(to:output)
    NSGraphicsContext.restoreGraphicsState()
}
func fit(_ source:NSImage, _ box:NSRect, cover:Bool = false) {
    let s = cover ? max(box.width/source.size.width,box.height/source.size.height) : min(box.width/source.size.width,box.height/source.size.height)
    let r = NSRect(x:box.midX-source.size.width*s/2,y:box.midY-source.size.height*s/2,width:source.size.width*s,height:source.size.height*s)
    NSGraphicsContext.saveGraphicsState(); box.clip(); source.draw(in:r); NSGraphicsContext.restoreGraphicsState()
}
func label(_ text:String, _ box:NSRect, size:CGFloat, ink:NSColor, fontName:String="AlegreyaRoman-ExtraBold") {
    let p = NSMutableParagraphStyle(); p.alignment = .center; p.lineSpacing = 0
    let font = NSFont(name:fontName,size:size) ?? NSFont.systemFont(ofSize:size,weight:.bold)
    let a = NSAttributedString(string:text,attributes:[.font:font,.foregroundColor:ink,.paragraphStyle:p])
    a.draw(in:box)
}
func phone(_ screen:NSImage, x:CGFloat, top:CGFloat, width:CGFloat, height:CGFloat) {
    let h = width * screen.size.height/screen.size.width
    let display = rect(x,top,width,h,height)
    let frame = display.insetBy(dx:-13,dy:-13)
    let shadow = NSShadow(); shadow.shadowColor = NSColor.black.withAlphaComponent(0.28); shadow.shadowBlurRadius = 30; shadow.shadowOffset = NSSize(width:0,height:-15)
    NSGraphicsContext.saveGraphicsState(); shadow.set()
    color(0x0F1E24).setFill(); NSBezierPath(roundedRect:frame,xRadius:45,yRadius:45).fill()
    NSGraphicsContext.restoreGraphicsState()
    color(0x799295).setStroke(); let rim=NSBezierPath(roundedRect:frame.insetBy(dx:2,dy:2),xRadius:43,yRadius:43);rim.lineWidth=2;rim.stroke()
    NSGraphicsContext.saveGraphicsState()
    NSBezierPath(roundedRect:display,xRadius:33,yRadius:33).addClip()
    screen.draw(in:display)
    NSGraphicsContext.restoreGraphicsState()
}
let mode = CommandLine.arguments.dropFirst().first ?? "all"
if mode == "icons" || mode == "all" {
    let master=image("icon-master.png"),foreground=image("icon-foreground.png")
    canvas(1024,1024,"Apple/icon-1024.png") { w,h in fit(master,NSRect(x:0,y:0,width:w,height:h)) }
    // Play Console requires 32-bit PNG, opaque pixels, < 1 MiB.
    canvas(512,512,"Google/icon-512.png",alpha:true) { w,h in fit(master,NSRect(x:0,y:0,width:w,height:h)) }
    canvas(1024,1024,"Google/adaptive-foreground.png",alpha:true) { w,h in fit(foreground,NSRect(x:w*0.16,y:h*0.16,width:w*0.68,height:h*0.68)) }
    canvas(1024,1024,"Google/adaptive-background.png") { w,h in
        NSGradient(starting:color(0x4F8B89),ending:color(0x153B4F))!.draw(in:NSRect(x:0,y:0,width:w,height:h),angle:75)
    }
}
if mode == "banners" || mode == "all" {
    canvas(1024,500,"Google/feature-1024x500.png") { w,h in fit(image("google-feature-master.png"),NSRect(x:0,y:0,width:w,height:h),cover:true) }
    canvas(3840,1646,"Apple/header-3840x1646.png") { w,h in fit(image("apple-header-master.png"),NSRect(x:0,y:0,width:w,height:h),cover:true) }
    canvas(1920,1280,"Apple/search-1920x1280.png") { w,h in fit(image("apple-search-master.png"),NSRect(x:0,y:0,width:w,height:h),cover:true) }
}
if mode == "screens" || mode == "all" {
    let posters:[(String,String,String,UInt32)] = [
        ("creative-day-background.png","Raw/iPhone/day.png","Every move\nmatters.",0x173B42),
        ("creative-night-background.png","Raw/iPhone/night.png","Find your way\nthrough.",0xFFF0CA),
        ("creative-sunset-background.png","Raw/iPhone/words.png","Play with shapes.\nThink in words.",0x173B42)
    ]
    let sizes:[(Int,Int,String)] = [(1320,2868,"Apple/iPhone-Large"),(1206,2622,"Apple/iPhone-Medium"),(1080,1920,"Google/Screenshots")]
    for (w,h,folder) in sizes {
        for (n,p) in posters.enumerated() {
            canvas(w,h,"\(folder)/0\(n+1)-creative.png") { W,H in
                fit(image(p.0),NSRect(x:0,y:0,width:W,height:H),cover:true)
                let scale=W/1320
                label("CASCADE",rect(W*0.08,H*0.025,W*0.84,55*scale,H),size:37*scale,ink:color(p.3),fontName:"Nunito-SemiBold")
                label(p.2,rect(W*0.04,H*0.055,W*0.92,270*scale,H),size:107*scale,ink:color(p.3))
                let phoneWidth = min(W*(n==0 ? 0.57 : 0.48),H*0.58/2.1727)
                let phoneX = n==0 ? W*0.5-phoneWidth/2 : n==1 ? W*0.90-phoneWidth : W*0.08
                phone(image(p.1),x:phoneX,top:H*0.20,width:phoneWidth,height:H)
            }
        }
        // Exact phone display rectangles measured in the original generated photographs.
        let photos:[(String,String,String,NSRect)] = [
            ("lifestyle-cafe-background.png","Raw/iPhone/day.png","Little puzzles.\nFamily moments.",NSRect(x:258,y:817,width:348,height:770)),
            ("lifestyle-park-background.png","Raw/iPhone/words.png","Good company.\nClever little puzzles.",NSRect(x:279,y:917,width:291,height:617))
        ]
        for (n,p) in photos.enumerated() {
            canvas(w,h,"\(folder)/0\(n+4)-lifestyle.png") { W,H in
                let source=image(p.0)
                // Crop scenery once, keeping the complete physical phone inside the canvas.
                let s=max(W/source.size.width,H/source.size.height)
                let topCrop=max(0,(p.3.minY+p.3.height)*s-H+W*0.035)
                let box=NSRect(x:W/2-source.size.width*s/2,y:H-source.size.height*s+topCrop,width:source.size.width*s,height:source.size.height*s)
                source.draw(in:box)
                precondition(p.3.width>0,"Measure the second photo screen first")
                let display=NSRect(x:box.minX+p.3.minX*s,y:box.maxY-(p.3.minY+p.3.height)*s,width:p.3.width*s,height:p.3.height*s)
                precondition(display.minX>=0 && display.maxX<=W && display.minY>=0 && display.maxY<=H,"Phone display would be cropped")
                NSGraphicsContext.saveGraphicsState()
                NSBezierPath(roundedRect:display,xRadius:32*s,yRadius:32*s).addClip()
                color(0x10282E).setFill(); display.fill()
                fit(image(p.1),display,cover:false)
                NSGraphicsContext.restoreGraphicsState()
                let play = W/H > 0.5
                label(p.2,rect(W*0.06,H*(play ? 0.013 : 0.035),W*0.88,H*0.17,H),size:W*(play ? 0.07 : 0.079),ink:color(0x173B42))
            }
        }
    }
}
if mode == "tablets" || mode == "all" {
    for (n,name) in ["day","night","advanced","words","bag"].enumerated() {
        let source=image("Raw/iPad/"+name+".png")
        precondition(source.size.width==2064 && source.size.height==2752,"Unexpected tablet dimensions")
        canvas(2064,2752,"Apple/iPad-13/0\(n+1)-\(name).png") { W,H in source.draw(in:NSRect(x:0,y:0,width:W,height:H)) }
    }
}
print("Store artwork export complete: \(mode)")
