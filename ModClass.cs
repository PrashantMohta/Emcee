using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using HkmpPouch;
using Satchel;

namespace Emcee
{
    public class Emcee : Mod
    {
        internal static Emcee Instance;
        internal static PipeClient PipeClient;
        internal RemoteMicrophone echoMic;

        public static byte EncodeSample(float sample)
        {
            byte encoded = (byte)Math.Round(((1f + sample) / 2f) * 255);
            return encoded;
        }

        public static float DecodeSample(byte encoded)
        {
            float decoded = ((float)(encoded/255f) * 2f) - 1f;
            return decoded;
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if(Instance == null)
            {
                PipeClient = new PipeClient("Emcee");
                Instance = this;
                PipeClient.OnRecieve += PipeClient_OnRecieve;
                On.HeroController.Start += HeroController_Start;
            }
        }

        private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            var playerMic = self.gameObject.GetAddComponent<MicrophoneManager>();
            echoMic = self.gameObject.GetAddComponent<RemoteMicrophone>();
        }

        private void PipeClient_OnRecieve(object sender, ReceivedEventArgs e)
        {
            
            DecodeAndPlay(e.Data.FromPlayer, e.Data.ExtraBytes);
        }

        private void DecodeAndPlay(ushort playerId,byte[] extraBytes)
        {
            if(extraBytes == null) return;
            float[] buffer = new float[extraBytes.Length];
            for (var i = 0; i < extraBytes.Length; i++)
            {
                buffer[i] = Emcee.DecodeSample(extraBytes[i]);
            }
            try { 
                var mic = PipeClient.ClientApi.ClientManager.GetPlayer(playerId).PlayerObject.GetAddComponent<RemoteMicrophone>();
                mic.AddSamples(buffer);
            } catch(Exception e)
            {
                Log(e);
            }
        }
    }
}