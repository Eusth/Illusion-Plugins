using UnityEngine;
using CSCore.Codecs;
using CSCore;
using CSCore.Streams.SampleConverter;
using System;

namespace AnanNaitegoran
{
    /// <summary>
    /// AudioClip wrapper that uses the CSCore library to stream audio files into Unity's AudioClips.
    /// </summary>
    public class CSCAudioClip : IDisposable
    {
        private string m_name;
        private IWaveSource m_source;
        private WaveToSampleBase m_decoder;
        private bool m_disposed = false;

        /// <summary>
        /// Gets the AudioClip that belongs to this CSCAudioClip.
        /// </summary>
        public AudioClip Clip { get; private set; }

        /// <summary>
        /// Creates a new CSCAudioClip from a network resource.
        /// </summary>
        /// <param name="uri"></param>
        public CSCAudioClip(Uri uri)
        {
            m_source = CodecFactory.Instance.GetCodec(uri);
            m_name = uri.AbsoluteUri;

            Initialize();
        }

        /// <summary>
        /// Creates a new CSCAudioClip from a local file.
        /// </summary>
        /// <param name="filename"></param>
        public CSCAudioClip(string filename)
        {
            m_source = CodecFactory.Instance.GetCodec(filename);
            m_name = filename;

            Initialize();
        }

        private void Initialize(bool stream = false)
        {
            switch (m_source.WaveFormat.BitsPerSample)
            {
                case 8:
                    m_decoder = new Pcm8BitToSample(m_source);
                    break;
                case 16:
                    m_decoder = new Pcm16BitToSample(m_source);
                    break;
                case 24:
                    m_decoder = new Pcm24BitToSample(m_source);
                    break;
                default:
                    Debug.LogError("No converter found!");
                    return;
            }

            Clip = AudioClip.Create(m_name,
                (int)(m_decoder.Length / m_decoder.WaveFormat.Channels),
                m_decoder.WaveFormat.Channels,
                m_decoder.WaveFormat.SampleRate,
                true,
                true,
                OnReadAudio,
                OnSetPosition);

        }

        public static AudioClip GetClip(string filename)
        {
            using (var source = CodecFactory.Instance.GetCodec(filename))
            {
                WaveToSampleBase decoder;
                switch (source.WaveFormat.BitsPerSample)
                {
                    case 8:
                        decoder = new Pcm8BitToSample(source);
                        break;
                    case 16:
                        decoder = new Pcm16BitToSample(source);
                        break;
                    case 24:
                        decoder = new Pcm24BitToSample(source);
                        break;
                    default:
                        throw new Exception("No decoder found");
                }

                using (decoder)
                {
                    var clip = AudioClip.Create(filename,
                        (int)(decoder.Length / decoder.WaveFormat.Channels),
                        decoder.WaveFormat.Channels,
                        decoder.WaveFormat.SampleRate,
                        true,
                        false);


                    var data = new float[decoder.Length];
                    decoder.Read(data, 0, (int)decoder.Length);
                    clip.SetData(data, 0);

                    return clip;
                }
            }
        }

        private void OnReadAudio(float[] data)
        {
            if (m_disposed) return;

            //Debug.LogFormat("Load Data: {0}", data.Length);
            m_decoder.Read(data, 0, data.Length);
        }

        private void OnSetPosition(int position)
        {
            if (m_disposed) return;

            //Debug.LogFormat("Set Position: {0}", position);
            m_decoder.Position = position * m_decoder.WaveFormat.Channels;
        }

        /// <summary>
        /// Frees resources taken by CSCore.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                m_decoder.Dispose();
                m_source.Dispose();

                m_disposed = true;
            }
        }

    }
}