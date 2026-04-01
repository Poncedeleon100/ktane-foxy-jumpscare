using Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FoxyJumpscareScript : MonoBehaviour
{

    public KMAudio Audio;

    public GameObject SoundObject;
    public GameObject[] FoxyJumpscare;

    public Sprite[] WitheredFoxy;
    public Sprite[] Mangle;
    public Sprite[] PhantomFoxy;
    public Sprite[] FuntimeFoxy;
    public AudioClip[] JumpscareClips;

    private readonly List<Sprite[]> _jumpscares = new List<Sprite[]>();
    private KMAudio.KMAudioRef _jumpscareSound;
    private bool _isCurrentlyJumpscaring = false;
    // Time in seconds each frame should remain on screen for a 30 FPS jumpscare
    private readonly float _frameDelay = 0.033333f;
    

    // Mod Settings
    private bool _isActive = true;
    private double _chance = 10000;
    private double _seconds = 1;
    private bool _unrestricted = false;
    private readonly List<int> _activeJumpscares = new List<int>();
    private FoxyJumpscareSettings _modConfig = new FoxyJumpscareSettings();

    void OnEnable()
    {
        GameEvents.OnGameStateChange += OnStateChange;
    }

    void OnDisable()
    {
        GameEvents.OnGameStateChange -= OnStateChange;
    }

    void Start()
    {
        _jumpscares.Add(WitheredFoxy);
        _jumpscares.Add(Mangle);
        _jumpscares.Add(PhantomFoxy);
        _jumpscares.Add(FuntimeFoxy);
    }

    void OnStateChange(SceneManager.State state)
    {
        CheckSettings();
        if (!_isActive)
        {
            return;
        }
        if ((state == SceneManager.State.Gameplay) || (_unrestricted && ((state == SceneManager.State.Setup) || (state == SceneManager.State.PostGame))))
        {
            StartCoroutine(CheckJumpscare());
        }
        else
        {
            Reset();
        }
    }

    IEnumerator CheckJumpscare()
    {
        while (!_isCurrentlyJumpscaring)
        {
            yield return new WaitForSeconds((float)_seconds);
            if (Random.Range(0, (int)_chance) == 0)
            {
                StartCoroutine(PlayJumpscare(_activeJumpscares.OrderBy(x => Random.Range(0, 1000)).First()));
            }
        }
    }

    IEnumerator PlayJumpscare(int jumpscare)
    {
        if (!_isCurrentlyJumpscaring)
        {
            _isCurrentlyJumpscaring = true;
            FoxyJumpscare[jumpscare].SetActive(true);
            _jumpscareSound = Audio.PlaySoundAtTransformWithRef(JumpscareClips[jumpscare].name, SoundObject.transform);
            foreach (Sprite frame in _jumpscares[jumpscare])
            {
                FoxyJumpscare[jumpscare].GetComponent<Image>().sprite = frame;
                yield return new WaitForSeconds(_frameDelay);
            }
            FoxyJumpscare[jumpscare].SetActive(false);
            _jumpscareSound.StopSound();
            _isCurrentlyJumpscaring = false;
        }
        StartCoroutine(CheckJumpscare());
    }

    void Reset()
    {
        StopAllCoroutines();
        if (_jumpscareSound != null)
        {
            _jumpscareSound.StopSound();
        }
        foreach (GameObject jumpscare in FoxyJumpscare)
        {
            jumpscare.SetActive(false);
        }
        _isCurrentlyJumpscaring = false;
    }

    void CheckSettings()
    {
        ModConfig<FoxyJumpscareSettings> foxyJumpscareJSON = new ModConfig<FoxyJumpscareSettings>("FoxyJumpscareSettings");
        _modConfig = foxyJumpscareJSON.Read();

        _chance = _modConfig.Chance;
        _seconds = _modConfig.Seconds;
        _unrestricted = _modConfig.Unrestricted;

        if (_modConfig.WitheredFoxy)
        {
            _activeJumpscares.Add(0);
        }
        if (_modConfig.Mangle)
        {
            _activeJumpscares.Add(1);
        }
        if (_modConfig.PhantomFoxy)
        {
            _activeJumpscares.Add(2);
        }
        if (_modConfig.FuntimeFoxy)
        {
            _activeJumpscares.Add(3);
        }

        if (_activeJumpscares.Count == 0)
        {
            _isActive = false;
        }
    }

    // Mod Settings
    public class FoxyJumpscareSettings
    {
        public double Chance = 10000;
        public double Seconds = 1;
        public bool Unrestricted = false;
        public bool WitheredFoxy = true;
        public bool Mangle = false;
        public bool PhantomFoxy = false;
        public bool FuntimeFoxy = false;
    }
    public static readonly Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "FoxyJumpscareSettings.json" },
            { "Name", "Foxy Jumpscare Settings" },
            { "Listings", new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Text", "All settings require a scene change before taking effect." },
                    { "Type", "Section" },
                },
                new Dictionary<string, object>
                {
                    { "Key", "Chance" },
                    { "Text", "Chance" },
                    { "Description", "The chance for a jumpscare to occur. Treated as 1 in Chance." },
                },
                new Dictionary<string, object>
                {
                    { "Key", "Seconds" },
                    { "Text", "Seconds" },
                    { "Description", "The delay in seconds before the mod checks for a jumpscare." },
                },
                new Dictionary<string, object>
                {
                    { "Key", "Unrestricted" },
                    { "Text", "Unrestricted" },
                    { "Description", "Enables the mod to jumpscare outside of gameplay." },
                },
                new Dictionary<string, object>
                {
                    { "Text", "If none of these settings are checked, the mod will be disabled." },
                    { "Type", "Section" },
                },
                new Dictionary<string, object>
                {
                    { "Key", "WitheredFoxy" },
                    { "Text", "Withered Foxy" },
                    { "Description", "Enables the Withered Foxy jumpscare." },
                },
                new Dictionary<string, object>
                {
                    { "Key", "Mangle" },
                    { "Text", "Mangle" },
                    { "Description", "Enables the Mangle jumpscare." },
                },
                new Dictionary<string, object>
                {
                    { "Key", "PhantomFoxy" },
                    { "Text", "Phantom Foxy" },
                    { "Description", "Enables the Phantom Foxy jumpscare." },
                },
                new Dictionary<string, object>
                {
                    { "Key", "FuntimeFoxy" },
                    { "Text", "Funtime Foxy" },
                    { "Description", "Enables the Funtime Foxy jumpscare." },
                },
            }
            },
        }
    };
}

class ModConfig<T> where T : new()
{
    public ModConfig(string filename, Action<Exception> onRead = null)
    {
        var settingsFolder = Path.Combine(Application.persistentDataPath, "Modsettings");
        // The persistent data path is different in the editor compared to the real game, so the Modsettings folder might not exist.
        if (Application.isEditor && !Directory.Exists(settingsFolder))
            Directory.CreateDirectory(settingsFolder);

        settingsPath = Path.Combine(settingsFolder, filename + ".json");
        OnRead = onRead;
    }

    private readonly string settingsPath;

    /// <summary>Serializes settings the same way it's written to the file. Supports settings that use enums.</summary>
    public static string SerializeSettings(T settings)
    {
        return JsonConvert.SerializeObject(settings, Formatting.Indented, new StringEnumConverter());
    }

    private static readonly object settingsFileLock = new object();

    /// <summary>Whether or not there has been a successful read of the settings file.</summary>
    public bool SuccessfulRead;
    /// <summary>Called every time the settings are read. Parameter is null if the read was successful or an exception if it wasn't.</summary>
    public Action<Exception> OnRead;

    public List<Action<JObject>> Migrations = new List<Action<JObject>>();

    /// <summary>
    /// Reads the settings from the settings file.
    /// If the settings couldn't be read, the default settings will be returned.
    /// </summary>
    public T Read()
    {
        try
        {
            lock (settingsFileLock)
            {
                if (!File.Exists(settingsPath))
                {
                    File.WriteAllText(settingsPath, SerializeSettings(new T()));
                }

                JObject jObject = JObject.Parse(File.ReadAllText(settingsPath));
                foreach (var migration in Migrations)
                {
                    migration(jObject);
                }

                T deserialized = jObject.ToObject<T>();
                if (deserialized == null)
                    deserialized = new T();

                SuccessfulRead = true;
                OnRead?.Invoke(null);
                return deserialized;
            }
        }
        catch (Exception e)
        {
            Debug.LogFormat("An exception has occurred while attempting to read the settings from {0}\nDefault settings will be used for the type of {1}.", settingsPath, typeof(T).ToString());
            Debug.LogException(e);

            SuccessfulRead = false;
            OnRead?.Invoke(e);
            return new T();
        }
    }

    /// <summary>
    /// Writes the settings to the settings file.
    /// To protect the user settings, this does nothing if the last read wasn't successful.
    /// </summary>
    public void Write(T value)
    {
        if (!SuccessfulRead)
            return;

        lock (settingsFileLock)
        {
            try
            {
                File.WriteAllText(settingsPath, SerializeSettings(value));
            }
            catch (Exception e)
            {
                Debug.LogFormat("Failed to write to {0}", settingsPath);
                Debug.LogException(e);
            }
        }
    }

    public override string ToString()
    {
        return SerializeSettings(Read());
    }
}
