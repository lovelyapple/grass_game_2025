using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResourceContainer : MonoBehaviour
{
    [Serializable]
    public class CharacterImage
    {
        public Characters Character;
        public Sprite CFResource;
        public Sprite CHResource;
    }
    [Serializable]
    public class SaddleImage
    {
        public SaddleType Saddle;
        public Sprite CFResource;
        public Sprite CHResource;
    }
    [Serializable]
    public class VehicleImage
    {
        public Vehicles Vehicle;
        public Sprite CFResource;
        public Sprite CHResource;
    }

    [SerializeField] private List<CharacterImage> CharacterImages;
    [SerializeField] private List<SaddleImage> SaddleImages;
    [SerializeField] private List<VehicleImage> VehicleImages;

    public static ResourceContainer Instance { get; private set; }
    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public Sprite GetCharacterImage(Characters character, bool full)
    {
        var container = CharacterImages.FirstOrDefault(x => x.Character == character);

        return full ? container.CFResource : container.CHResource;
    }
    public Sprite GetSaddleImage(SaddleType saddle, bool full)
    {
        var container = SaddleImages.FirstOrDefault(x => x.Saddle == saddle);

        return full ? container.CFResource : container.CHResource;
    }
    public Sprite GetVehicleImage(Vehicles vehicle, bool full)
    {
        var container = VehicleImages.FirstOrDefault(x => x.Vehicle == vehicle);

        return full ? container.CFResource : container.CHResource;
    }
}
