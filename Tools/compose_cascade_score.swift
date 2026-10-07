// Original CASCADE chamber scores. No third-party composition or recording.
// macOS supplies the playback instruments; the bank itself is not copied.
// swift -module-cache-path /tmp/cascade-score-cache Tools/compose_cascade_score.swift <output-folder>
import AVFoundation
import AudioToolbox
import Foundation

struct Note { let time: Double; let end: Double; let instrument: Int; let pitch: UInt8; let velocity: UInt8 }
struct Theme {
    let name: String; let bpm: Double; let root: Int; let minor: Bool; let lead: Int; let pluck: Int; let warmth: Float
}
let output = CommandLine.arguments[1]
let selectedTheme = CommandLine.arguments.firstIndex(of: "--theme").map { CommandLine.arguments[$0 + 1] }
let balanceMenu = CommandLine.arguments.contains("--menu-balanced")
try FileManager.default.createDirectory(atPath: output, withIntermediateDirectories: true)
let themes = [
    Theme(name: "Menu_HomeAmongClouds", bpm: 84, root: 62, minor: false, lead: 0, pluck: 1, warmth: 0.18),
    Theme(name: "WindGarden_PaperWings", bpm: 92, root: 62, minor: false, lead: 3, pluck: 6, warmth: 0.16),
    Theme(name: "RainMill_RainOnCedar", bpm: 76, root: 57, minor: true, lead: 0, pluck: 1, warmth: 0.24),
    Theme(name: "CopperOrchard_GoldenAfternoon", bpm: 104, root: 60, minor: false, lead: 3, pluck: 1, warmth: 0.14),
    Theme(name: "CloudPost_LettersOnTheBreeze", bpm: 96, root: 65, minor: false, lead: 7, pluck: 2, warmth: 0.15),
    Theme(name: "GlassLagoon_ReflectedMoon", bpm: 80, root: 62, minor: true, lead: 3, pluck: 2, warmth: 0.23),
    Theme(name: "MossArchive_StoriesInTheLeaves", bpm: 72, root: 59, minor: true, lead: 3, pluck: 6, warmth: 0.22),
    Theme(name: "SnowSpire_WarmLightInSnow", bpm: 78, root: 64, minor: false, lead: 0, pluck: 2, warmth: 0.26),
    Theme(name: "GreatBeacon_TheWayHome", bpm: 100, root: 62, minor: false, lead: 3, pluck: 1, warmth: 0.20)
]
// Piano, koto, harp, flute, strings, cello, nylon guitar, vibraphone.
let programs: [UInt8] = [0, 107, 46, 73, 48, 42, 24, 11]
let pans: [Float] = [-0.16, 0.28, -0.27, -0.07, 0.16, -0.22, 0.20, 0.12]
// Each location has its own tune, rather than merely transposing one recording.
// The keeper's rising-third motif returns in the first garden and final beacon.
let verses = [
    [0, 2, 4, 2, 1, -1, -3, -1, -2, 0, 2, 0, -3, -1, 1, -1],
    [0, 2, 4, 5, 4, 2, 1, 0, 2, 4, 6, 4, 5, 2, 1, 0],
    [0, -1, -3, -2, 0, 1, 2, 1, 0, 2, 1, -1, -2, -3, -1, 0],
    [2, 4, 5, 4, 2, 1, 0, 2, 4, 5, 7, 5, 4, 2, 1, 0],
    [4, 2, 0, 1, 2, 4, 5, 4, 2, 1, 2, 0, -1, 1, 2, 0],
    [0, 2, 1, 4, 2, 1, -1, 0, 2, 5, 4, 2, 1, -1, -2, 0],
    [-3, -1, 0, 2, 1, 0, -2, -1, 0, 1, 4, 2, 0, -1, -3, -1],
    [4, 2, 1, 0, 2, 5, 4, 2, 1, 0, -1, 1, 2, 4, 2, 0],
    [0, 2, 4, 2, 5, 4, 2, 1, 0, 2, 5, 4, 2, 1, -1, 0]
]
let refrains = [
    [4, 5, 4, 2, 3, 2, 1, 0, 2, 4, 5, 4, 2, 1, 0, 0],
    [4, 7, 6, 5, 4, 2, 4, 5, 7, 6, 4, 2, 5, 4, 1, 0],
    [2, 4, 5, 4, 2, 1, 0, -1, 2, 3, 4, 2, 1, -1, 0, 0],
    [5, 7, 9, 7, 5, 4, 2, 4, 5, 4, 2, 1, 4, 2, 0, 0],
    [5, 4, 2, 4, 7, 5, 4, 2, 4, 5, 4, 1, 2, 1, 0, 0],
    [5, 4, 2, 5, 4, 1, 2, 0, 4, 2, 1, -1, 2, 1, 0, 0],
    [0, 2, 4, 2, 1, 4, 5, 4, 2, 0, -1, 1, 2, 1, -1, 0],
    [7, 5, 4, 2, 5, 4, 2, 1, 4, 2, 1, 0, 2, 1, 0, 0],
    [4, 5, 7, 6, 5, 4, 2, 4, 5, 7, 9, 7, 5, 4, 2, 0]
]
let harmonies = [0, 5, 3, 4, 0, 2, 3, 4, 5, 3, 0, 4, 3, 4, 0, 0]

func compose(_ theme: Theme, index: Int, balancedMenu: Bool = false) -> ([Note], Double) {
    var notes = [Note]()
    let unit = 30 / theme.bpm // eighth note; six per bar
    let scale = theme.minor ? [0, 2, 3, 5, 7, 9, 10] : [0, 2, 4, 5, 7, 9, 11]
    func pitch(_ degree: Int, octave: Int = 0) -> Int {
        let oct = Int(floor(Double(degree) / 7))
        let pos = ((degree % 7) + 7) % 7
        return theme.root + scale[pos] + oct * 12 + octave * 12
    }
    func add(_ start: Double, _ duration: Double, _ inst: Int, _ key: Int, _ velocity: Int, _ swing: Double = 0) {
        let human = sin(start * 1.71 + Double(index * 3 + inst)) * 0.012 + swing
        let onset = max(0, start * unit + human)
        notes.append(Note(time: onset, end: onset + duration * unit, instrument: inst,
                          pitch: UInt8(clamping: key), velocity: UInt8(clamping: velocity)))
    }
    for bar in 0..<32 {
        let start = Double(bar * 6), b = bar % 16, chorus = bar >= 16
        let degree = harmonies[b]
        // Six-eight motion with breathing spaces, bass on the large beats.
        for beat in [0, 3] { add(start + Double(beat), 2.7, 5, pitch(degree, octave: -2), chorus ? 48 : 39) }
        for voice in 0..<3 { add(start, 5.7, 4, pitch(degree + voice * 2, octave: -1), chorus ? 38 : 27) }
        let pattern = chorus ? [0, 4, 2, 6, 4, 2] : [0, 2, 4, 2, 4, 2]
        for step in 0..<6 {
            add(start + Double(step), 1.4, theme.pluck, pitch(degree + pattern[step], octave: -1),
                (step == 0 || step == 3 ? 60 : 43) + (chorus ? 7 : 0))
        }
        // Established CASCADE contour becomes a question and a rising response.
        let melody = chorus ? refrains[index] : verses[index]
        let m = (bar % 8) * 2
        let lead = (index == 0 && chorus) ? 3 : theme.lead
        let dynamic = bar % 8 == 7 ? -10 : 0
        // A sustained flute is louder than the A-section piano at the same MIDI velocity.
        // Keep the tune, but let its first two B bars open as a phrase rather than a level jump.
        let entrance = balancedMenu && chorus ? (bar == 16 ? -12 : bar == 17 ? -6 : 0) : 0
        add(start, 2.65, lead, pitch(melody[m], octave: 1), 67 + dynamic + (chorus ? 6 : 0) + entrance)
        // Leave the end of every four-bar phrase open for an acoustic reply.
        if bar % 4 != 3 { add(start + 3, 2.35, lead, pitch(melody[m + 1], octave: 1), 58 + dynamic + entrance) }
        else {
            add(start + 3, 1.35, 0, pitch(degree + 4), 47)
            add(start + 4.5, 1.3, 0, pitch(degree + 2), 40)
        }
        // A restrained second melodic voice appears in the B section.
        if chorus && bar % 2 == 0 {
            add(start + 1.5, 3.5, 2, pitch(degree + 2), 42)
            add(start + 4.5, 1.4, 2, pitch(degree + 4), 36)
        }
    }
    return (notes, 32 * 6 * unit)
}

func render(_ notes: [Note], length: Double, name: String, wet: Float, balancedMenu: Bool = false) throws {
    let engine = AVAudioEngine()
    let format = AVAudioFormat(standardFormatWithSampleRate: 44100, channels: 2)!
    var samplers = [AVAudioUnitSampler]()
    for (i, program) in programs.enumerated() {
        let sampler = AVAudioUnitSampler(); engine.attach(sampler)
        engine.connect(sampler, to: engine.mainMixerNode, format: format)
        try sampler.loadSoundBankInstrument(at: URL(fileURLWithPath: "/System/Library/Components/CoreAudio.component/Contents/Resources/gs_instruments.dls"),
            program: program, bankMSB: UInt8(kAUSampler_DefaultMelodicBankMSB), bankLSB: 0)
        sampler.stereoPan = pans[i] * 100
        sampler.masterGain = i == 4 ? -8 : i == 5 ? -5 : -2
        if balancedMenu && i == 3 { sampler.masterGain = -8.5 }
        samplers.append(sampler)
    }
    let reverb = AVAudioUnitReverb(); reverb.loadFactoryPreset(.mediumHall); reverb.wetDryMix = wet * 100
    engine.attach(reverb); engine.connect(engine.mainMixerNode, to: reverb, format: format)
    engine.connect(reverb, to: engine.outputNode, format: format)
    engine.mainMixerNode.outputVolume = 0.56
    try engine.enableManualRenderingMode(.offline, format: format, maximumFrameCount: 2048)
    try engine.start()
    var events = [(Int64, Bool, Note)]()
    for note in notes {
        events.append((Int64(note.time * 44100), true, note))
        events.append((Int64(note.end * 44100), false, note))
    }
    events.sort { $0.0 == $1.0 ? (!$0.1 && $1.1) : $0.0 < $1.0 }
    let file = try AVAudioFile(forWriting: URL(fileURLWithPath: output + "/" + name + ".wav"), settings: [
        AVFormatIDKey: kAudioFormatLinearPCM, AVSampleRateKey: 44100, AVNumberOfChannelsKey: 2,
        AVLinearPCMBitDepthKey: 16, AVLinearPCMIsFloatKey: false, AVLinearPCMIsBigEndianKey: false])
    let buffer = AVAudioPCMBuffer(pcmFormat: format, frameCapacity: 2048)!
    let total = Int64((length + 3) * 44100); var event = 0; var retries = 0
    while engine.manualRenderingSampleTime < total {
        let now = engine.manualRenderingSampleTime
        while event < events.count && events[event].0 <= now {
            let (_, on, note) = events[event]
            if on { samplers[note.instrument].startNote(note.pitch, withVelocity: note.velocity, onChannel: 0) }
            else { samplers[note.instrument].stopNote(note.pitch, onChannel: 0) }
            event += 1
        }
        let next = event < events.count ? events[event].0 : total
        let frames = AVAudioFrameCount(min(2048, min(total - now, next - now)))
        if frames == 0 { continue }
        let status = try engine.renderOffline(frames, to: buffer)
        if status == .success { try file.write(from: buffer); retries = 0 }
        else { retries += 1; if retries > 500 { throw NSError(domain: "CASCADE audio render", code: 1) } }
    }
    engine.stop(); print("Rendered \(name): \(Int(length)) s + acoustic tail")
}

for (index, theme) in themes.enumerated() where selectedTheme == nil || selectedTheme == theme.name {
    let balanced = balanceMenu && index == 0
    let (score, length) = compose(theme, index: index, balancedMenu: balanced)
    try render(score, length: length, name: theme.name + (balanced ? "Balanced" : ""), wet: theme.warmth, balancedMenu: balanced)
}
// Short, purposeful acoustic reactions, with proper release rather than bleeps.
let cues: [(String, [Int], Int, Double)] = CommandLine.arguments.contains("--scores-only") ? [] : [
    ("Repair_Chimes", [62, 66, 69], 2, 0.08), ("Defuse_Release", [69, 66, 62], 0, 0.10),
    ("Bridge_Unfold", [62, 69, 74], 1, 0.11), ("Success_Home", [62, 66, 69, 74], 2, 0.13),
    ("Unlock_Turn", [66, 69], 7, 0.06)
]
for (name, keys, instrument, step) in cues {
    let notes = keys.enumerated().map { i, key in Note(time: Double(i) * step, end: Double(i) * step + 0.35,
        instrument: instrument, pitch: UInt8(key), velocity: UInt8(70 - i * 3)) }
    try render(notes, length: 0.65, name: name, wet: 0.14)
}
