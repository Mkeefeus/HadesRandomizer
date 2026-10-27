using System.IO;
using EldenRingParamsEditor;
using Hades.Constants;
using Hades.Services;
using System.Windows;

namespace Hades.Formats;

class QOLegacy : IRandomizerFormat
{
    public string Id => "qolegacy";
    public string DisplayName => "QOLegacy";
    public string Me3File => "qolegacy.me3";
    private EldenRingLauncherService _launcherService = new EldenRingLauncherService();

    public void Exec(
        string baseSeed,
        Action<string>? statusCallback = null,
        Action<string>? seedCallback = null,
        IRandomizerOptions? options = null
    )
    {
        statusCallback?.Invoke("Randomizing...");
        var opts = options as QOLegacyOptions ?? new QOLegacyOptions();
        if (baseSeed == "")
        {
            var seed = Utils.RandoUtils.GenerateRandomString();
            seedCallback?.Invoke(seed);
            baseSeed = seed;
        }

        // Formatting seed
        baseSeed = $"qolegacy_{baseSeed}";

        var regulationFilepath = Path.Combine(
            GlobalConstants.ModEngineWorkingDirectory,
            Id,
            "bingo",
            "regulation.bin"
        );
        if (!File.Exists(regulationFilepath))
        {
            var msg = $"regulation.bin not found at {Path.GetFullPath(regulationFilepath)}";
            throw new RandomizerException(msg);
        }

        var editor = ParamsEditor.ReadFromRegulationPath(regulationFilepath);

        // Randomization Logic
        var swTotal = System.Diagnostics.Stopwatch.StartNew();

        var urr = new UniversalReplacementRandomizer.OptimizedReplacementRandomizer(
            "qolegacy",
            Utils.RandoUtils.GetStableSeed(baseSeed)
        );

        // Weapons
        var allNormalWeapons = Weapons.GuaranteedWeapons.Concat(Weapons.ChanceWeapons).ToList();

        var weaponCategories = allNormalWeapons
            .GroupBy(w => w.Category)
            .OrderBy(g => g.Key.ToString())
            .ToList();
        foreach (var group in weaponCategories)
        {
            var category = group.Key;
            var weaponsInCategory = group.ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            WeaponRandomizerService.RandomizeWeaponWorldGroup(
                editor,
                urr,
                $"weapon_category_{category}",
                weaponsInCategory,
                weaponsInCategory
            );
            sw.Stop();
        }

        // Remembrance Weapons
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeRemembranceWeaponShops(
                editor,
                urr,
                "shop_remembrance_weapons"
            );
            sw.Stop();
        }

        // Spells
        var worldSpells = Spells.WorldSpells;
        var remSpells = Spells.RemembranceSpells;
        var spellGroups = worldSpells
            .GroupBy(s => s.Type)
            .OrderBy(g => g.Key.ToString())
            .ToList();
        foreach (var group in spellGroups)
        {
            var spellType = group.Key;
            var spellsInType = group.ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            SpellRandomizerService.RandomizeSpellWorldGroup(
                editor,
                urr,
                $"spell_type_{spellType}",
                spellsInType,
                spellsInType
            );
            sw.Stop();
        }

        // Weapons in Shop
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeWeaponShopsByCategory(
                editor,
                urr,
                "shop_weapon_category_",
                includeRemembranceInPool: false
            );
            sw.Stop();
        }
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeSpellShopsByCategory(editor, urr);
            sw.Stop();
        }
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ShopRandomizerService.RandomizeRemembranceSpellShops(editor, urr);
            sw.Stop();
        }

        // Class Armor
        var classResult = ArmorRandomizerService.GetRandomArmoredClasses(
            baseSeed,
            ArmorLocation.Both
        );
        randomizeClasses(editor, classResult);

        // Starting Stats
        StartingClassService.RandomizeStats(editor, baseSeed + "_statrando", (className, seed) => StatRandomizer(className, seed, opts));

        // Class Weapons + deficit description (WeaponName (-X) with 2H Str buff)
        StartingClassService.RandomizeAllStartingWeaponsWithDescriptions(
            editor,
            baseSeed + "_classweapons",
            Id
        );

        editor.WriteToRegulationPath(regulationFilepath);

        statusCallback?.Invoke("Successfully randomized!");
    }

    public void Launch()
    {
        _launcherService.LaunchEldenRingFromMe3File(Me3File);
    }

    public StartingStats StatRandomizer(string className, string seed, QOLegacyOptions opts)
    {
        static int GenerateStat(string seed, int min, int max, double mean, double stdDev)
        {
            const int maxAttempts = 300;
            for (var attempt = 0; ; attempt++)
            {
                var s = seed + "_stat" + attempt;

                var u1 = (Utils.RandoUtils.GetRandomNumber(s + "_u1", 10000) + 1) / 10001.0;
                var u2 = Utils.RandoUtils.GetRandomNumber(s + "_u2", 10000) / 10000.0;

                var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                var stat = (int)Math.Round(mean + stdDev * z);

                if (stat >= min && stat <= max)
                    return stat;
                if (attempt >= maxAttempts)
                    throw new RandomizerException("Failed to generate stats, attempt maximum reached.");
            }
        }

        bool betterStats = opts.BetterStartingStats;
        int total = betterStats ? 104 : 88;
        int statMin = betterStats ? 8 : 6;
        int statMax = betterStats ? 20 : 18;
        double mean = betterStats ? 13.0 : 11.0;
        double stdDev = betterStats ? 2.5 : 3;
        const int maxAttempts = 300;

        string[] keys = { "vigor", "mind", "end", "str", "dex", "int", "fai", "arc" };
        int[] stats = new int[keys.Length];


        for (var attempt = 0; ; attempt++)
        {
            var sum = 0;
            for (var i = 0; i < keys.Length; i++)
            {
                string classedSeed = $"{seed}_{className}_{keys[i]}_{attempt}";
                stats[i] = GenerateStat(classedSeed, statMin, statMax, mean, stdDev);
                sum += stats[i];
            }
            if (sum == total)
                break;
            if (attempt >= maxAttempts)
                throw new RandomizerException("Failed to generate stats, attempt maximum reached.");
        }

        int vigorMod = betterStats ? 5 : 3;
        int mindMod = betterStats ? 5 : 3;
        short runeLevel = (short)(betterStats ? 10 : 8);

        return new StartingStats
        {
            Vigor = stats[0] + vigorMod,
            Mind = stats[1] - mindMod,
            Endurance = stats[2],
            Strength = stats[3],
            Dexterity = stats[4],
            Intelligence = stats[5],
            Faith = stats[6],
            Arcane = stats[7],
            RuneLevel = runeLevel
        };
    }

    private void randomizeClasses(ParamsEditor editor, ClassArmorResults results)
    {
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Vagabond"], results.Vagabond);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Warrior"], results.Warrior);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Hero"], results.Hero);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Astrologer"], results.Astrologer);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Prophet"], results.Prophet);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Confessor"], results.Confessor);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Bandit"], results.Bandit);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Samurai"], results.Samurai);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Prisoner"], results.Prisoner);
        randomizeClass(editor, GlobalConstants.CharaInitClassMap["Wretch"], results.Wretch);
        randomizeClass(
            editor,
            GlobalConstants.CharaInitClassMap["Heavy Knight"],
            results.HeavyKnight
        );
        randomizeClass(
            editor,
            GlobalConstants.CharaInitClassMap["Idus Knight"],
            results.IdusKnight
        );
    }

    private void randomizeClass(ParamsEditor editor, int classId, ArmorResults result)
    {
        editor.SetInitialEquipArm(
            classId,
            Armors.Get(Gauntlets.All, ArmorLocation.Both).ElementAt(result.Arms).Id
        );
        editor.SetInitialEquipHelm(
            classId,
            Armors.Get(Helms.All, ArmorLocation.Both).ElementAt(result.Helm).Id
        );
        editor.SetInitialEquipLeg(
            classId,
            Armors.Get(Greaves.All, ArmorLocation.Both).ElementAt(result.Legs).Id
        );
        editor.SetInitialEquipTorso(
            classId,
            Armors.Get(ChestArmor.All, ArmorLocation.Both).ElementAt(result.Chest).Id
        );
    }
}
