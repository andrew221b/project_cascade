"""Original sparse cricket stridulation; no recording, sample library or model audio.

Two narrow insect resonances use unequal pulses, tiny pitch jitter and soft air
texture. The runtime leaves 9–15 seconds of quiet between these short phrases.
"""
from pathlib import Path
import math
import random
import struct
import wave


ROOT = Path(__file__).resolve().parents[1]
DESTINATION = ROOT / "CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Audio/Atmosphere/NightCrickets.wav"
RATE = 24000
DURATION = 4.2


def build():
    rng = random.Random(806626)
    signal = [0.0] * round(RATE * DURATION)
    # A nearby cricket calls twice; a softer neighbour answers after a pause.
    for start, frequency, gain, pulse_count in (
        (.22, 4280, .22, 4), (.93, 4210, .19, 5),
        (2.17, 3680, .085, 3), (2.88, 4250, .17, 4),
    ):
        for pulse in range(pulse_count):
            onset = start + pulse * rng.uniform(.050, .054)
            length = rng.uniform(.023, .029)
            pitch = frequency + rng.uniform(-35, 35)
            offset = round(onset * RATE)
            for index in range(round(length * RATE)):
                t = index / RATE
                envelope = math.sin(math.pi * t / length) ** 1.5
                phase = 2 * math.pi * (pitch * t + 17 * t * t / length)
                grain = .86 + .14 * math.sin(2 * math.pi * 155 * t)
                value = gain * envelope * grain * (
                    math.sin(phase) + .14 * math.sin(phase * 2 + .35)
                    + .055 * rng.uniform(-1, 1)
                )
                signal[offset + index] += value
    pcm = [round(max(-1, min(1, value)) * 32767) for value in signal]
    with wave.open(str(DESTINATION), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(struct.pack(f"<{len(pcm)}h", *pcm))
    print(f"NightCrickets.wav: {DURATION:.1f}s, mono PCM16/{RATE} Hz, peak={max(abs(v) for v in pcm) / 32767:.4f}, endpoints={pcm[0]}/{pcm[-1]}")


if __name__ == "__main__":
    build()
