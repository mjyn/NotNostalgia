using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using static NotNostalgia.Utilities.KeysoundFilenameUtilities;
using NotNostalgia;
using NotNostalgia.Models;
using System.IO;

public class PlaySceneScript : MonoBehaviour
{
    public GameObject NotePrefab;
    public GameObject SlideNotePrefab;
    public GameObject LongNoteBodyPrefab;
    public GameObject DoriruNoteBodyPrefab;
    public GameObject AudioSourcePrefab;
    public Text DebugText;
    public int MaxSources;


    private MusicScore _musicScore;
    private List<MusicScoreSubNote> _hiddenSubNotes;
    private float _backtrackStartTime;
    private Dictionary<string, AudioClip> _audioClipDict;
    private bool _gameStarted;

    private AudioSource _audioSourceBg;
    private List<AudioSourceInfo> _audioSources;

    private CurrentNotesManager _currentNotesManager;

    private Dictionary<int, bool> _lastFrameKeyStatus;
    private Dictionary<int, bool> _currentFrameKeyStatus;

    private int _debug_samplecount;
    private int _debug_sourcesused;
    private int _debug_audioSourceMaxOutCount;


    void LoadMusicScore(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(MusicScore));
        StreamReader xmlreader = new StreamReader(Path.Combine(path, "m_t0017_sumidagawa_02extreme.xml"));

        var musicscore = serializer.Deserialize(xmlreader) as MusicScore;

        //start load sounds
        var wavefiles = Directory.GetFiles(path, "*.wav", SearchOption.TopDirectoryOnly);
        _audioClipDict = new Dictionary<string, AudioClip>();
        foreach (var file in wavefiles)
        {
            using (WWW www = new WWW($"file://{file}"))
            {
                while (!www.isDone)
                {

                }
                var a = www.GetAudioClip(false, false);
                //var filenamearray = Path.GetFileName(file).Split('_');
                //int number = int.Parse(filenamearray[filenamearray.Length - 1]);
                _audioClipDict.Add(Path.GetFileNameWithoutExtension(file), a);
            }
        }

        wavefiles = Directory.GetFiles(Path.Combine(path, "Generic"), "*.wav", SearchOption.AllDirectories);
        foreach (var file in wavefiles)
        {
            using (WWW www = new WWW($"file://{file}"))
            {
                while (!www.isDone)
                {

                }
                var a = www.GetAudioClip(false, false);
                //var filenamearray = Path.GetFileName(file).Split('_');
                //int number = int.Parse(filenamearray[filenamearray.Length - 1]);
                _audioClipDict.Add(Path.GetFileNameWithoutExtension(file), a);
            }
        }
        //end load sounds


        //var hiddennote = musicscore.NoteData.Where(q => q.Hand == 2).ToList();
        var hiddennote = musicscore.NoteData;

        _hiddenSubNotes = new List<MusicScoreSubNote>();

        foreach (var note in hiddennote)
        {
            foreach (var subnote in note.SubNoteData)
            {
                _hiddenSubNotes.Add(subnote);
            }
        }
        _hiddenSubNotes = _hiddenSubNotes.OrderBy(q => q.StartTimingMsec).ToList();
        //处理隐藏音符


        musicscore.NoteData.RemoveAll(q => q.Hand == 2);
        _musicScore = musicscore;
    }

    public void OnGameStartButtonClick()
    {
        var btn = GameObject.Find("StartButton");
        btn.SetActive(false);
        GameStart();
    }

    void GameStart()
    {
        _backtrackStartTime = Time.time;
        _gameStarted = true;


        _audioSourceBg.clip = _audioClipDict["_backtrack"];
        _audioSourceBg.Play();
    }

    void PlaySubNote(MusicScoreSubNote subNote, bool isProtected = false)
    {

        string str = _musicScore.TrackInfo.First(q => q.Index == subNote.TrackIndex).Name;
        str += $"_{GetFileSuffix(subNote.ScalePiano)}";

        //0~30 = 31    if[31]=maxedout
        for (int i = 0; i < MaxSources; i++)
        {
            if (i == MaxSources - 1)//即从头到尾都inuse
            {
                _audioSources = _audioSources.OrderBy(q => q.StartTime).ToList();
                var old = _audioSources.First(q => q.Protected == false);
                old.AudioSource.Stop();
                old.AudioSource.clip = _audioClipDict[str];
                old.StartTime = subNote.StartTimingMsec / 1000f;
                old.Endtime = subNote.EndTimingMsec / 1000f;
                old.AudioSource.Play();
                _debug_audioSourceMaxOutCount++;
                break;
            }
            if (!_audioSources[i].InUse)
            {
                _audioSources[i].InUse = true;
                _audioSources[i].AudioSource.clip = _audioClipDict[str];
                _audioSources[i].StartTime = subNote.StartTimingMsec / 1000f;
                _audioSources[i].Endtime = subNote.EndTimingMsec / 1000f;
                _audioSources[i].AudioSource.Play();
                _debug_sourcesused++;
                break;
            }
        }

        _debug_samplecount++;
    }

    // Use this for initialization
    void Start()
    {
        _gameStarted = false;
        LoadMusicScore($@"D:\work\NNSongs");
        _debug_samplecount = 0;
        _debug_sourcesused = 0;
        _debug_audioSourceMaxOutCount = 0;
        _audioSources = new List<AudioSourceInfo>();
        _currentNotesManager = new CurrentNotesManager(NotePrefab, LongNoteBodyPrefab, DoriruNoteBodyPrefab, SlideNotePrefab);
        _lastFrameKeyStatus = new Dictionary<int, bool>();
        for (int i = 1; i < 29; i++)
        {
            _lastFrameKeyStatus.Add(i, false);
        }
        _currentFrameKeyStatus = new Dictionary<int, bool>();


        //begin create audiosources
        GameObject asourcepre = Instantiate(AudioSourcePrefab);
        _audioSourceBg = asourcepre.GetComponent<AudioSource>();

        for (int i = 0; i < MaxSources - 1; i++)
        {
            GameObject a = Instantiate(AudioSourcePrefab);
            var info = new AudioSourceInfo()
            {
                AudioSource = a.GetComponent<AudioSource>(),
                InUse = false
            };
            _audioSources.Add(info);
        }
    }

    // Update is called once per frame
    void Update()
    {
        DebugText.text = "";


        if (_gameStarted)
        {
            float currenttime = Time.time - _backtrackStartTime;

            //stop keyoto
            foreach (var item in _audioSources)
            {
                if ((item.InUse) && (item.Endtime < currenttime))
                {
                    item.AudioSource.Stop();
                    item.InUse = false;
                    _debug_sourcesused--;
                }
            }
            //end stop keyoto


            //Play bg keyoto (hand=2)
            while (_hiddenSubNotes.First().StartTimingMsec / 1000.0 < currenttime)
            {
                PlaySubNote(_hiddenSubNotes.First());
                _hiddenSubNotes.Remove(_hiddenSubNotes.First());
            }

            //End play bg

            //start generate note
            while (_musicScore.NoteData[0].StartTimingMsec / 1000.0 < currenttime + 1f)
            {
                
                _currentNotesManager.AddNote(_musicScore.NoteData[0]);
                _musicScore.NoteData.Remove(_musicScore.NoteData[0]);
            }


            //end generate note


            //start keydetect
            _currentFrameKeyStatus.Clear();
            for (int i = 1; i < 29; i++)
            {
                _lastFrameKeyStatus.Add(i, false);
            }
            foreach (var touch in Input.touches)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    //Select Stage
                    for (int i = 1; i < 29; i++)
                    {
                        if (Physics.Raycast(ray, out hit))
                        {
                            //Select Stage
                            if (hit.transform.name == "Panel" + i.ToString())
                            {
                                DebugText.text += $"Panel{i.ToString()}hit";
                                _currentFrameKeyStatus[i] = true;
                            }
                        }
                    }
                }
                //DebugText.text += (touch.position.x + " " + touch.position.y + "\n");
            }
            //end keydetect

            //start hantei
            _currentNotesManager.Hantei(_lastFrameKeyStatus, _currentFrameKeyStatus, currenttime);

            //end hantei


            DebugText.text += $"\n{currenttime.ToString()}";
            DebugText.text += $"\nFPS:{1 / Time.deltaTime}";
            DebugText.text += $"\n{_debug_samplecount} samples";
            DebugText.text += $"\n{_debug_sourcesused} audiosources inuse, maxedout {_debug_audioSourceMaxOutCount} times";
        }

    }
}
