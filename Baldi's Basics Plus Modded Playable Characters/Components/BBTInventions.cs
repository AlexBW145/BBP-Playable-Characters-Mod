using BBP_Playables.Core;
using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;
using UnityEngine;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace BBP_Playables.Modded.BBTimes;

internal static class BBTInventions
{
    internal static void DoStuff()
    {
        GameObject basketballshooter = new GameObject("BasketballCannon_Tinkerneer", typeof(BoxCollider));
        basketballshooter.GetComponent<BoxCollider>().isTrigger = true;
        basketballshooter.GetComponent<BoxCollider>().size = Vector3.one;
        var shooterbod = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shooterbod.transform.SetParent(basketballshooter.transform);
        shooterbod.transform.localScale = new(4f, 5f, 4f);
        shooterbod.transform.localPosition = Vector3.up * 3.2f;
        var mat = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "Locker_Red"));
        mat.SetMainTexture(Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(x => x.name == "machineTexture"));
        shooterbod.GetComponent<Renderer>().SetMaterial(mat);
        GameObject.Destroy(shooterbod.GetComponent<Collider>());
        var shootercann = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shootercann.transform.SetParent(basketballshooter.transform);
        shootercann.transform.localScale = new(1f, 1f, 3f);
        shootercann.transform.localPosition = new(-0.5f, 4.13f, 0.55f);
        mat = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "Locker_Red"));
        mat.SetMainTexture(Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(x => x.name == "cannonTexture"));
        shootercann.GetComponent<Renderer>().SetMaterial(mat);
        // Copy and pasted from BBT because I can't use that function to this script.
        Mesh mesh = shootercann.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = [
        new (0, 1f, 0),
            new (0, 0, 0),
            new (1.004f, 1f, 0),
            new (1f, 0, 0),

            new (0, 0, 1f),
            new (1f, 0, 1f),
            new (0, 1f, 1f),
            new (1f, 1f, 1f),

            new (0, 1f, 0),
            new (1f, 1f, 0),

            new (0, 1f, 0),
            new(0, 1f, 1f),

            new(1f, 1f, 0),
            new(1f, 1f, 1f),
        ];
        mesh.triangles = [
        0, 2, 1, // front
			1, 2, 3,
            4, 5, 6, // back
			5, 7, 6,
            6, 7, 8, //top
			7, 9 ,8,
            1, 3, 4, //bottom
			3, 5, 4,
            1, 11,10,// left
			1, 4, 11,
            3, 12, 5,//right
			5, 12, 13


    ];
        mesh.uv = [
        new(0, 0.66f),
            new(0.25f, 0.66f),
            new(0, 0.33f),
            new(0.25f, 0.34f),

            new(0.5f, 0.66f),
            new(0.5f, 0.34f),
            new(0.75f, 0.65f),
            new(0.75f, 0.34f),

            new(1, 0.66f),
            new(1, 0.34f),

            new(0.25f, 1),
            new(0.5f, 1),

            new(0.25f, 0),
            new(0.5f, 0),
        ];
        mesh.Optimize();
        mesh.RecalculateNormals();
        GameObject.Destroy(shootercann.GetComponent<Collider>());
        var shooterbase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shooterbase.transform.SetParent(basketballshooter.transform);
        shooterbase.transform.localScale = new(5f, 4f, 5f);
        shooterbase.transform.localPosition = Vector3.zero;
        shooterbase.GetComponent<Renderer>().SetMaterial(Resources.FindObjectsOfTypeAll<Material>().ToList().Last(x => x.name == "BlackUnlit"));
        GameObject.Destroy(shooterbase.GetComponent<Collider>());
        basketballshooter.CreateTinkerneeringObject<BasketBallThrower>(Plugin.info, "Basketball Shooter", "An iffy version of the school's basketball shooter, it does not hit you, but can push back others at a slow pace.", [ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("Basketball")).value, ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value, ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("FidgetSpinner")).value], true, RoomCategory.Hall);
        basketballshooter.GetComponent<BasketBallThrower>().audMan = basketballshooter.AddComponent<PropagatedAudioManager>();
        basketballshooter.GetComponent<BasketBallThrower>().audMan.audioDevice = basketballshooter.AddComponent<AudioSource>();
        basketballshooter.GetComponent<BasketBallThrower>().audBoom = Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.soundClip.name == "shootBoom");
        var annoying = basketballshooter.GetComponent<BasketBallThrower>().audMan.audioDevice;
        annoying.spatialBlend = 1;
        annoying.rolloffMode = AudioRolloffMode.Custom;
        annoying.maxDistance = 150;
        annoying.dopplerLevel = 0;
        annoying.spread = 0;
    }
}

public class BasketBallThrower : TinkerneerObject
{
    [SerializeField] private ITM_Basketball basketBallPre = ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("Basketball")).value.item as ITM_Basketball;
    [SerializeField] internal PropagatedAudioManager audMan;
    [SerializeField] internal SoundObject audBoom;
    private float cooldownToShoot = 8f;
    private EnvironmentController ec;
    public override void Create(ItemManager itm)
    {
        ec = itm.pm.ec;
        transform.position = new(transform.position.x, 0f, transform.position.z);
        transform.forward = CoreGameManager.Instance.GetCamera(itm.pm.playerNumber).transform.forward;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.Round(transform.eulerAngles.y / 90f) * 90f, transform.rotation.eulerAngles.z);
        int flag = 0;
        while (Physics.OverlapBox(transform.position + transform.forward * 5f, Vector3.one * 4.5f).ToList().Exists(x => x.CompareTag("Wall")) && flag++ < 4)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 90f, transform.rotation.eulerAngles.z);
        ec.CellFromPosition(transform.position).HardCover(CellCoverage.Down);
    }

    private static FieldInfo _maxHitsBeforeDying = AccessTools.DeclaredField(typeof(ITM_Basketball), "maxHitsBeforeDying");
    private static FieldInfo _lifeTime = AccessTools.DeclaredField(typeof(ITM_Basketball), "lifeTime");
    void Update()
    {
        cooldownToShoot -= Time.deltaTime * ec.EnvironmentTimeScale;
        if (cooldownToShoot < 0f)
        {
            cooldownToShoot += UnityEngine.Random.Range(8f, 10f);
            ITM_Basketball iTM_Basketball = Instantiate(basketBallPre);
            iTM_Basketball.Setup(ec, transform.forward, transform.position + transform.forward * 7f + Vector3.up * 5f, ec.mainHall, 0.6f);
            _maxHitsBeforeDying.SetValue(iTM_Basketball, 1);
            _lifeTime.SetValue(iTM_Basketball, 8f);
            for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                iTM_Basketball.GetComponent<Entity>().IgnoreEntity(CoreGameManager.Instance.GetPlayer(i).plm.Entity, true);
            audMan.PlaySingle(audBoom);
        }
    }
}
