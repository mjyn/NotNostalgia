using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NotNostalgia.Models
{
    public class AudioSourceInfo
    {
        public AudioSource AudioSource { get; set; }
        public float StartTime { get; set; }
        public float Endtime { get; set; }
        public bool InUse { get; set; }
        /// <summary>
        /// wont be stopped when all audio sources are in use
        /// </summary>
        public bool Protected { get; set; }
    }
}
