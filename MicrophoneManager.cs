using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Satchel;

namespace Emcee
{
    internal class MicrophoneManager : MonoBehaviour
    {
        public AudioClip stream;
        private int pos = 0, lastPos = 0;
        internal DateTime lastTime = DateTime.Now, currentTime = DateTime.Now;
        public void Start()
        {
            foreach (var device in Microphone.devices)
            {
                Debug.Log("Name: " + device);
            }
            Microphone.GetDeviceCaps("External Microphone (Realtek(R) Audio", out var minf, out var maxf);
            Debug.Log("minf:"+minf);
            stream = Microphone.Start(null, true, 360, 44100); // Mono
        }

        public void Update()
        {
            currentTime = DateTime.Now;
            pos = Microphone.GetPosition(null);
            if (pos > 0 && (currentTime - lastTime).TotalMilliseconds > 500f)
            {
                if (lastPos > pos) lastPos = 0;

                if (pos - lastPos > 0)
                {
                    // Allocate the space for the new sample.
                    int len = (pos - lastPos) * stream.channels;
                    float[] samples = new float[len];
                    stream.GetData(samples, lastPos);
                    SendToServer(samples);
                    lastPos = pos;
                }
                lastTime = currentTime;
            }
        }

        private void SendToServer(float[] samples)
        {
            byte[] buffer = new byte[samples.Length];
            bool hasSound = false;
            for (var i =0; i < samples.Length; i++)
            {
                buffer[i] = Emcee.EncodeSample(samples[i]);
                if(Math.Abs(samples[i]) > 0.01)
                {
                    hasSound = true;
                }
            }
            if (hasSound) { 
                Emcee.PipeClient.Broadcast("Mic", "i",buffer,true,false);
            }
            //Emcee.Instance.echoMic.AddSamples(samples);
        }

        public void OnDestroy()
        {
            Microphone.End(null);
        }
    }
}