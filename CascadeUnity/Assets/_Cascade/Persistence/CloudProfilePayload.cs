using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Cascade.Persistence
{
    // Versioned transport; old JSON remains accepted. Never removes economic receipts.
    public static class CloudProfilePayload
    {
        public const int MaxDecodedBytes=8*1024*1024,MaxWireBytes=1536*1024;
        const string Prefix="gz1:";
        public static string Pack(string json)
        {
            if(json==null)return null;
            byte[] bytes=Encoding.UTF8.GetBytes(json);
            if(bytes.Length>MaxDecodedBytes)throw new InvalidDataException("Cloud profile exceeds decoded budget.");
            if(bytes.Length<32768)return json;
            using(var output=new MemoryStream()) {
                using(var zip=new GZipStream(output,CompressionLevel.Fastest,true))zip.Write(bytes,0,bytes.Length);
                string packed=Prefix+Convert.ToBase64String(output.ToArray());
                return packed.Length<bytes.Length?packed:json;
            }
        }
        public static string Unpack(string payload,int maxPackedBytes=MaxWireBytes)
        {
            if(payload==null)return null;
            if(!payload.StartsWith(Prefix,StringComparison.Ordinal)) {
                if(Encoding.UTF8.GetByteCount(payload)>MaxDecodedBytes)throw new InvalidDataException("Cloud profile exceeds decoded budget.");
                return payload;
            }
            if(payload.Length>maxPackedBytes)throw new InvalidDataException("Compressed profile exceeds wire budget.");
            byte[] compressed;
            try {compressed=Convert.FromBase64String(payload.Substring(Prefix.Length));}
            catch(FormatException error){throw new InvalidDataException("Invalid compressed profile.",error);}
            using(var input=new MemoryStream(compressed))using(var zip=new GZipStream(input,CompressionMode.Decompress))using(var output=new MemoryStream()) {
                var buffer=new byte[8192];int count;
                while((count=zip.Read(buffer,0,buffer.Length))>0) {
                    if(output.Length+count>MaxDecodedBytes)throw new InvalidDataException("Expanded profile exceeds budget.");
                    output.Write(buffer,0,count);
                }
                return new UTF8Encoding(false,true).GetString(output.ToArray());
            }
        }
    }
}
