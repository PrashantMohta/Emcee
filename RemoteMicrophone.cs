using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Satchel;
using static Satchel.GameObjectUtils;
namespace Emcee
{
    internal class RemoteMicrophone : MonoBehaviour
    {
        public AudioClip clip;
        public int position;
        public AudioSource audioSource;
        public List<float> buffer = new List<float>();
        public System.Random rnd = new System.Random();
        public void Start()
        {
            audioSource = gameObject.GetAddComponent<AudioSource>();
            audioSource.volume = 1f;
            clip = AudioClip.Create("NC", 44100*360 , 1, 44100, true, pcmreadercallback);
            audioSource.clip = clip;
            audioSource.Play();
        }

        private void pcmreadercallback(float[] data)
        {
            Emcee.Instance.Log("callback!");
            
            for (int count = 0; count < data.Length;count++)
            {
                var baseline = 0.001f * ((rnd.Next(200) / 100f) - 1f); ;
                if (position < buffer.Count) { 
                    data[count] =  Math.Abs(buffer[position]) < 0.1f || Math.Abs(buffer[position]) > 0.9f ? buffer[position] * 0.9f :  buffer[position] * 1.2f;
                    position++;
                }
                else
                {
                    data[count] = baseline;
                }
            }
            if(position >= buffer.Count)
            {
                position = 0;
                buffer = new List<float>();
            }
        }

        public void AddSamples(float[] data)
        {
            foreach(float sample in data)
            {
                buffer.Add(sample);
            }
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
