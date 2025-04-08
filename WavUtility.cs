using UnityEngine;
using System.IO;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile)
    {
        using (MemoryStream memoryStream = new MemoryStream(wavFile))
        using (BinaryReader reader = new BinaryReader(memoryStream))
        {
            // RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF")
            {
                Debug.LogError("Invalid WAV file, missing RIFF header");
                return null;
            }

            int chunkSize = reader.ReadInt32();
            string wave = new string(reader.ReadChars(4));

            // Format chunk
            string fmt = new string(reader.ReadChars(4));
            int subchunk1Size = reader.ReadInt32();
            ushort audioFormat = reader.ReadUInt16();
            ushort numChannels = reader.ReadUInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            ushort blockAlign = reader.ReadUInt16();
            ushort bitsPerSample = reader.ReadUInt16();

            // Data chunk
            string dataString = new string(reader.ReadChars(4));
            if (dataString != "data")
            {
                Debug.LogError("Invalid WAV file, missing data chunk");
                return null;
            }

            int dataSize = reader.ReadInt32();

            // Read audio data
            byte[] byteArray = reader.ReadBytes(dataSize);

            // Convert byte array to float array
            float[] floatArray = new float[byteArray.Length / 2];
            for (int i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = (short)(byteArray[i * 2] | byteArray[i * 2 + 1] << 8) / 32768.0f;
            }

            // Create AudioClip
            AudioClip audioClip = AudioClip.Create("Generated Audio", floatArray.Length, numChannels, sampleRate, false);
            audioClip.SetData(floatArray, 0);
            return audioClip;
        }
    }
}
