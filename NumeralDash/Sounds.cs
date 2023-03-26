using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace NumeralDash;

static class Sounds
{
    // https://pixabay.com/sound-effects/pickup-item-64282/
    static readonly SoundEffectInstance[] s_pickupSounds = new SoundEffectInstance[5];

    // https://pixabay.com/sound-effects/concrete-footsteps-6752/
    static readonly SoundEffectInstance[] s_stepSounds = new SoundEffectInstance[5];

    static readonly SoundEffectInstance[] s_levelSounds = new SoundEffectInstance[5];
    static int s_prevLevelSoundIndex;

    static Sounds()
    {
        for (int i = 0; i < 5; i++)
        {
            s_pickupSounds[i] = FromFile("pickups", "pickup" + (i + 1));
            s_stepSounds[i] = FromFile("steps", "step" + (i + 1));
            s_levelSounds[i] = FromFile("levels", "level" + (i + 1));
        }

        s_prevLevelSoundIndex = Program.GetRandomIndex(s_stepSounds.Length);
    }

    static public SoundEffectInstance PickUp
    {
        get
        {
            int i = Program.GetRandomIndex(s_pickupSounds.Length);
            return s_pickupSounds[i];
        }
    }

    static public SoundEffectInstance Level
    {
        get
        {
            int i;
            do i = Program.GetRandomIndex(s_levelSounds.Length);
            while (i == s_prevLevelSoundIndex);
            s_prevLevelSoundIndex = i;
            return s_levelSounds[i];
        }
    }

    static public SoundEffectInstance Step
    {
        get
        {
            int i = Program.GetRandomIndex(s_stepSounds.Length);
            return s_stepSounds[i];
        }
    }

    static SoundEffectInstance FromFile(string folder, string fileName)
    {
        string path = Path.Combine("Resources", "Sounds", folder, $"{fileName}.wav");
        return SoundEffect.FromFile(path).CreateInstance();
    }

    static SoundEffectInstance FromFile(string fileName)
    {
        string path = Path.Combine("Resources", "Sounds", $"{fileName}.wav");
        return SoundEffect.FromFile(path).CreateInstance();
    }
}