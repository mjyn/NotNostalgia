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
        public GameObject DoriruNoteBodyPrefab;
        public GameObject SlideNotePrefab;


        private float _badTiming, _goodTiming, _greatTiming, _pGreatTiming;

        public CurrentNotesManager(GameObject normalNotePrefab, GameObject longNoteBodyPrefab, GameObject doriruNoteBodyPrefab, GameObject slideNotePrefab)
        {
            NormalNotePrefab = normalNotePrefab;
            LongNoteBodyPrefab = longNoteBodyPrefab;
            DoriruNoteBodyPrefab = doriruNoteBodyPrefab;
            SlideNotePrefab = slideNotePrefab;
            CurrentNotes = new List<Note>();

            _pGreatTiming = 0.03f;
            _greatTiming = 0.05f;
            _goodTiming = 0.15f;
            _badTiming = 0.3f;
        }
        public void AddNote(MusicScoreNote musicScoreNote)
        {
            switch (musicScoreNote.NoteType)
            {

                case 4:
                case 12:
                    //temp slide
                    GameObject slide = UnityEngine.Object.Instantiate(SlideNotePrefab);
                    slide.GetComponent<NoteController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex);

                    var slidenote = new Note()
                    {
                        NoteType = NoteTypeEnum.SlideNote,
                        StartTime = musicScoreNote.StartTimingMsec / 1000f,
                        MinIndex = musicScoreNote.MinKeyIndex,
                        GameObject = slide,
                        MaxIndex = musicScoreNote.MaxKeyIndex
                    };
                    foreach (var subnote in musicScoreNote.SubNoteData)
                    {
                        slidenote.SubNotes.Add(subnote);
                    }
                    CurrentNotes.Add(slidenote);
                    break;
                case 0:
                case 8:
                    //normal note
                    GameObject gobj = UnityEngine.Object.Instantiate(NormalNotePrefab);
                    gobj.GetComponent<NoteController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex);

                    var note = new Note()
                    {
                        NoteType = NoteTypeEnum.NormalNote,
                        StartTime = musicScoreNote.StartTimingMsec / 1000f,
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
                    lnbody.GetComponent<LongNoteBodyController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex, musicScoreNote.GateTimeMsec / 1000f);
                    var longnote = new Note()
                    {
                        NoteType = NoteTypeEnum.LongNote,
                        StartTime = musicScoreNote.StartTimingMsec / 1000f,
                        MinIndex = musicScoreNote.MinKeyIndex,
                        MaxIndex = musicScoreNote.MaxKeyIndex,
                        GameObject = lnhead,
                        LnBodyGameObject = lnbody
                    };
                    foreach (var subnote in musicScoreNote.SubNoteData)
                    {
                        longnote.SubNotes.Add(subnote);
                    }
                    CurrentNotes.Add(longnote);
                    break;
                case 64:
                case 72:
                    //doriru note
                    GameObject drrhead = UnityEngine.Object.Instantiate(NormalNotePrefab);
                    drrhead.GetComponent<NoteController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex);
                    GameObject drrbody = UnityEngine.Object.Instantiate(DoriruNoteBodyPrefab);
                    drrbody.GetComponent<DoriruNoteBodyController>().SetProperties(musicScoreNote.MinKeyIndex, musicScoreNote.MaxKeyIndex, musicScoreNote.GateTimeMsec / 1000f);
                    var dorirunote = new Note()
                    {
                        NoteType = NoteTypeEnum.DoriruNote,
                        StartTime = musicScoreNote.StartTimingMsec / 1000f,
                        MinIndex = musicScoreNote.MinKeyIndex,
                        MaxIndex = musicScoreNote.MaxKeyIndex,
                        GameObject = drrhead,
                        LnBodyGameObject = drrbody
                    };
                    foreach (var subnote in musicScoreNote.SubNoteData)
                    {
                        dorirunote.SubNotes.Add(subnote);
                    }
                    CurrentNotes.Add(dorirunote);
                    break;
            }
        }
        public void Hantei(Dictionary<int, bool> lastFrame, Dictionary<int, bool> currentFrame, float currentTime)
        {
            for (int i = 1; i < 29; i++)
            {
                if ((currentFrame[i] == true) && (lastFrame[i] == false))
                {
                    //keypress
                    GameObject nearestNote;
                    foreach (var note in CurrentNotes)
                    {
                        if (!((note.MinIndex <= i) && (note.MaxIndex >= i)))
                            continue;
                        else
                        {
                            //in region
                            if (Math.Abs(note.StartTime - currentTime) <= _badTiming)
                            {
                                aaa
                            }
                        }
                    }
                }
            }
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
        //is lnbody or slideconnector
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
    //public enum ControlEnum
    //{
    //    Press,
    //    Release
    //}
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
        LongNote,
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
