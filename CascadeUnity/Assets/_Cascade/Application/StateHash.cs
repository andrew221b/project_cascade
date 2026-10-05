using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Domain;

namespace Cascade.Application
{
    public static class StateHash
    {
        public static string Compute(LevelDefinition level, PuzzleState state)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (state == null) throw new ArgumentNullException(nameof(state));
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write(1); // fingerprint format, independent of runtime GetHashCode
                    writer.Write(level.PuzzleId); writer.Write(level.CampaignNodeId);
                    writer.Write(level.RulesVersion); writer.Write(level.ContentHash);
                    writer.Write(state.Row.Count);
                    foreach (var symbol in state.Row)
                    { writer.Write((int)symbol.Kind); writer.Write(symbol.Locale); writer.Write(symbol.Value); }
                    writer.Write(state.ActiveFloor); writer.Write(state.Durability);
                    writer.Write((int)state.BombPhase); writer.Write(state.BombTicks);
                    writer.Write(state.KeyUsed); writer.Write(state.RepairUsed); writer.Write(state.BridgeUsed);
                    writer.Write(state.StampBits); writer.Write(state.DefusedCount);
                    writer.Write(state.ExplodedCount); writer.Write(state.AbandonedCount); writer.Write((int)state.Status);
                    if(level.ReachCharges>0||level.Floors.Any(f=>f.Powerup!=null)){writer.Write("longreach-v1");writer.Write(state.ReachCharges);}
                    if(level.Floors.Any(f=>f.Powerup!=null)){writer.Write("pickups-v1");writer.Write(state.PowerupUsed);writer.Write(state.FrozenTicks);}
                }
                using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
