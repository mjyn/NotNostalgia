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

        private Dictionary<int, LaneStatus> _laneStatus;
        private PlaySceneScript _pss;
        private float _missTiming, _goodTiming, _justTiming, _pJustTiming;

        private HanteiEnum getHantei(float time1, float time2)
        {
            float abs = Math.Abs(time1 - time2);
            if (abs <= _pJustTiming)
                return HanteiEnum.PJust;
            else if (abs <= _justTiming)
                return HanteiEnum.Just;
            else if (abs <= _goodTiming)
                return HanteiEnum.Good;
            else if (abs <= _missTiming)
                return HanteiEnum.Miss;
            else
                return HanteiEnum.InActive;
        }
        public CurrentNotesManager(PlaySceneScript pss, GameObject normalNotePrefab, GameObject longNoteBodyPrefab, GameObject doriruNoteBodyPrefab, GameObject slideNotePrefab)
        {
            _pss = pss;
            NormalNotePrefab = normalNotePrefab;
            LongNoteBodyPrefab = longNoteBodyPrefab;
            DoriruNoteBodyPrefab = doriruNoteBodyPrefab;
            SlideNotePrefab = slideNotePrefab;
            CurrentNotes = new List<Note>();
            _laneStatus = new Dictionary<int, LaneStatus>();
            for (int i = 1; i < 29; i++)
            {
                _laneStatus.Add(i, new LaneStatus()
                {
                    Note = null,
                    Status = LaneStatusEnum.Idle
                });
            }

            _pJustTiming = 0.03f;
            _justTiming = 0.05f;
            _goodTiming = 0.15f;
            _missTiming = 0.3f;
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
        public void RemoveNote(Note note)
        {
            CurrentNotes.Remove(note);
            note.GameObject.SetActive(false);
        }
        public void Hantei(Dictionary<int, bool> lastFrame, Dictionary<int, bool> currentFrame, float currentTime)
        {
            for (int i = 1; i < 29; i++)
            {
                if ((currentFrame[i] == true) && (lastFrame[i] == false))
                {
                    //keypress
                    if (_laneStatus[i].Status != LaneStatusEnum.Idle)
                        throw new Exception("keypress not idle exception");

                    Note nearestNote = null;
                    foreach (var note in CurrentNotes)
                    {
                        if ((note.MinIndex <= i) && (note.MaxIndex >= i))
                        {
                            //in region
                            if (getHantei(note.StartTime, currentTime) != HanteiEnum.InActive)
                            {
                                //in timing
                                if (nearestNote == null)
                                    nearestNote = note;
                                else if (nearestNote.StartTime > note.StartTime)
                                    nearestNote = note;
                            }
                        }
                    }
                    if (nearestNote == null)
                        continue;

                    //hantei nearest(active)note
                    var hantei = getHantei(nearestNote.StartTime, currentTime);

                    if (hantei == HanteiEnum.Miss)
                    {
                        RemoveNote(nearestNote);
                    }
                    else
                    {
                        if (nearestNote.NoteType == NoteTypeEnum.NormalNote)
                        {
                            float deltanotetime = nearestNote.StartTime - currentTime;
                            for (int j = 0; j < nearestNote.SubNotes.Count; j++)
                            {
                                nearestNote.SubNotes[j].EndTimingMsec = nearestNote.SubNotes[j].EndTimingMsec - (int)Math.Round(deltanotetime * 1000.0);
                                nearestNote.SubNotes[j].StartTimingMsec = nearestNote.SubNotes[j].StartTimingMsec - (int)Math.Round(deltanotetime * 1000.0);
                                _pss.QueuedSubNotes.Add(nearestNote.SubNotes[j]);
                            }
                            RemoveNote(nearestNote);
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
        //in sec
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public int MinIndex { get; set; }
        public int MaxIndex { get; set; }
        //in msec
        public List<MusicScoreSubNote> SubNotes { get; set; }

        //public HanteiEnum HanteiStart(float currentTime)
        //{
        //    if (Math.Abs(StartTime - currentTime) < 0.03)
        //        return HanteiEnum.PJust;
        //    else if (Math.Abs(StartTime - currentTime) < 0.05)
        //        return HanteiEnum.Just;
        //    else if (Math.Abs(StartTime - currentTime) < 0.07)
        //        return HanteiEnum.Good;
        //    else
        //        return HanteiEnum.Miss;
        //}
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
        Miss,
        InActive
    }
    public enum NoteTypeEnum
    {
        NormalNote,
        LongNote,
        DoriruNote,
        SlideNote
    }
    public enum LaneStatusEnum
    {
        Idle,
        InLong,
        InDoriru
    }
    public class LaneStatus
    {
        public LaneStatusEnum Status { get; set; }
        //in what note
        public Note Note { get; set; }
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
