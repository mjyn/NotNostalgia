using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotNostalgia.Models;
using UnityEngine;

namespace NotNostalgia
{
    public class CurrentNotesManager
    {
        public List<Note> CurrentNotes;
        public GameObject NormalNotePrefab;
        public GameObject LongNoteBodyPrefab;

        public CurrentNotesManager(GameObject normalNotePrefab)
        {
            NormalNotePrefab = normalNotePrefab;
            CurrentNotes = new List<Note>();
        }
        public void AddNote(MusicScoreNote musicScoreNote)
        {
            switch (musicScoreNote.KeyKind)
            {
                case 0:
                case 8:
                    //normal note
                    GameObject gobj = UnityEngine.Object.Instantiate(NormalNotePrefab);
                    gobj.GetComponent<NoteController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex);

                    var note = new Note()
                    {
                        NoteType = NoteTypeEnum.NormalNote,
                        StartTime = musicScoreNote.StartTimingMsec / 1000,
                        MinIndex = musicScoreNote.MinKeyIndex,
                        GameObject = gobj,
                        MaxIndex = musicScoreNote.MaxKeyIndex
                    };
                    foreach (var subnote in musicScoreNote.SubNoteData)
                    {
                        note.SubNotes.Add(subnote);
                    }
                    CurrentNotes.Add(note);
                    break;
                case 2:
                case 10:
                    //long note
                    GameObject lnhead = UnityEngine.Object.Instantiate(NormalNotePrefab);
                    lnhead.GetComponent<NoteController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex);
                    GameObject lnbody = UnityEngine.Object.Instantiate(LongNoteBodyPrefab);
                    lnhead.GetComponent<LongNoteBodyController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex, musicScoreNote.GateTimeMsec / 1000f);

                    break;
            }
        }
        public void Hantei()
        {

        }
    }
    public class Note
    {
        public Note()
        {
            SubNotes = new List<MusicScoreSubNote>();
        }
        public NoteTypeEnum NoteType { get; set; }
        public GameObject GameObject { get; set; }
        public GameObject LnBodyGameObject { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public int MinIndex { get; set; }
        public int MaxIndex { get; set; }
        public List<MusicScoreSubNote> SubNotes { get; set; }

        public HanteiEnum HanteiStart(float currentTime)
        {
            if (Math.Abs(StartTime - currentTime) < 0.03)
                return HanteiEnum.PJust;
            else if (Math.Abs(StartTime - currentTime) < 0.05)
                return HanteiEnum.Just;
            else if (Math.Abs(StartTime - currentTime) < 0.07)
                return HanteiEnum.Good;
            else
                return HanteiEnum.Miss;
        }
        //public HanteiEnum HanteiEnd(float currentTime)
        //{
        //    if (Math.Abs(EndTime - currentTime) < 0.03)
        //        return HanteiEnum.PJust;
        //    else if (Math.Abs(EndTime - currentTime) < 0.05)
        //        return HanteiEnum.Just;
        //    else if (Math.Abs(EndTime - currentTime) < 0.07)
        //        return HanteiEnum.Good;
        //    else
        //        return HanteiEnum.Miss;
        //}
    }
    public enum HanteiEnum
    {
        PJust,
        Just,
        Good,
        Near,
        Miss
    }
    public enum NoteTypeEnum
    {
        NormalNote,
        LongNoteHead,
        LongNoteBody,
        DoriruNote,
        SlideNote
    }
    /*
     * public NoteTypeEnum NoteType
        {
            get
            {
                switch (MusicScoreNote.NoteType)
                {
                    case 0:
                    case 8:
                        return NoteTypeEnum.NormalNote;
                    case 2:
                    case 10:
                        return NoteTypeEnum.LongNote;
                    case 4:
                    case 12:
                        return NoteTypeEnum.SlideNote;
                    case 64:
                    case 72:
                        return NoteTypeEnum.DoriruNote;
                    default:
                        throw new Exception("unknown note type");
                }
            }
        }
        */

}
