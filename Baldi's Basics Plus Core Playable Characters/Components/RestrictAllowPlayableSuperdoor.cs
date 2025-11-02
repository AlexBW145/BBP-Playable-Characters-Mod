using BBP_Playables.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBP_Playables.Modded.LevelEditor;

public class RestrictAllowPlayableSuperdoor : Door
{
    [SerializeField]
    internal MeshRenderer door;

    [SerializeField]
    internal BoxCollider collider;

    [SerializeField]
    internal Sprite mapUnlockedSprite;

    [SerializeField]
    internal Sprite mapLockedSprite;

    [SerializeField] internal SpriteRenderer[] renders, playables;

    protected MapTile aMapTile, bMapTile;

    [SerializeField]
    private float collisionHeight = 6f;

    public float originalHeight;

    private float targetHeight;

    [SerializeField] internal List<PlayableCharacter> whiteblacklist = new List<PlayableCharacter>();

    private bool blockingAudio;

    [SerializeField] internal bool DisallowMode = false;

    private void Awake()
    {
        originalHeight = door.transform.localPosition.y;
    }

    public override void Initialize()
    {
        base.Initialize();
        collider.enabled = false;

        aMapTile = ec.map.AddExtraTile(aTile.position);
        aMapTile.SpriteRenderer.sprite = mapUnlockedSprite;
        aMapTile.SpriteRenderer.color = aTile.room.color;
        aMapTile.transform.rotation = direction.ToUiRotation();
        bMapTile = ec.map.AddExtraTile(bTile.position);
        bMapTile.SpriteRenderer.sprite = mapUnlockedSprite;
        bMapTile.transform.rotation = direction.GetOpposite().ToUiRotation();
        bMapTile.SpriteRenderer.color = bTile.room.color;
    }

    public void SetList(List<PlayableCharacter> list)
    {
        if (PlayableCharsPlugin.IsRandom && PlayableCharsPlugin.Instance.Character.componentType == typeof(PlayableRandomizer))
            PlayableRandomizer.RandomizePlayable();
        whiteblacklist = list;
        if ((DisallowMode && whiteblacklist.Contains(PlayableCharsPlugin.Instance.Character)) || (!DisallowMode && !whiteblacklist.Contains(PlayableCharsPlugin.Instance.Character)))
            Shut();

        for (int i = 0; i < renders.Length; i++)
            renders[i].sprite = DisallowMode ? LoaderAdds.restrictIcon : null;
        UpdateRenders();
    }

    private void UpdateRenders()
    {
        for (int i = 0; i < playables.Length; i++)
            playables[i].sprite = whiteblacklist.Count == 0 ? LoaderAdds.noPlayableIcon : whiteblacklist[Mathf.RoundToInt(UnityEngine.Random.Range(0f, whiteblacklist.Count - 1))].sprselect;
    }

    private float time = 9f;
    private void Update()
    {
        if (time > 0)
            time -= Time.deltaTime * ec.PlayerTimeScale;
        else
        {
            time = 9f;
            UpdateRenders();
        }
    }

    public override void UnInitialize()
    {
        base.UnInitialize();
        Open(true, false);
        Destroy(aMapTile.gameObject);
        Destroy(bMapTile.gameObject);
    }

    public override void Shut()
    {
        base.Shut();
        targetHeight = 0f;

        aMapTile.SpriteRenderer.sprite = mapLockedSprite;
        bMapTile.SpriteRenderer.sprite = mapLockedSprite;

        Vector3 vector = door.transform.position;
        vector.y = 0f;
        door.transform.position = vector;
        collider.enabled = true;
        if (!blockingAudio)
        {
            aTile.Mute(direction, block: true);
            bTile.Mute(direction.GetOpposite(), block: true);
            blockingAudio = true;
        }
    }

    public override void Open(bool cancelTimer, bool makeNoise)
    {
        base.Open(cancelTimer, makeNoise);
        targetHeight = originalHeight;

        aMapTile.SpriteRenderer.sprite = mapUnlockedSprite;
        bMapTile.SpriteRenderer.sprite = mapUnlockedSprite;

        Vector3 vector = door.transform.position;
        vector.y = originalHeight;
        door.transform.position = vector;
        collider.enabled = false;
        if (blockingAudio)
        {
            aTile.Mute(direction, block: false);
            bTile.Mute(direction.GetOpposite(), block: false);
            blockingAudio = false;
        }
    }
}

public class Structure_RestrictAllowPlayableSuperdoor : StructureBuilder
{
    [SerializeField]
    internal Door defaultDoorPrefab;

    [SerializeField]
    internal bool defaultIfTrapped = true;

    public override void Load(List<StructureData> data)
    {
        base.Load(data);
        foreach (StructureData datum in data)
        {
            if (datum.prefab?.GetComponent<RestrictAllowPlayableSuperdoor>() != null)
            {
                PlaceDoor(datum.prefab.GetComponent<Door>(), datum.position, datum.direction, checkBlock: false, out var newDoor);
                var lister = (PlayableIDs.PlayableIDList)datum.data;
                List<PlayableCharacter> playables = lister.GetPlayables().ToList();
                newDoor.GetComponent<RestrictAllowPlayableSuperdoor>().SetList(playables);
            }
        }
    }

    protected bool PlaceDoor(Door prefab, IntVector2 position, Direction direction, bool checkBlock, out GameObject newDoor)
    {
        IntVector2 intVector = direction.ToIntVector2();
        Cell cell = ec.CellFromPosition(position);
        Cell cell2 = ec.cells[position.x + intVector.x, position.z + intVector.z];
        Door door = UnityEngine.Object.Instantiate(prefab, cell.ObjectBase);
        newDoor = door.gameObject;
        door.transform.rotation = direction.ToRotation();
        door.ec = ec;
        door.position = cell.position;
        door.bOffset = direction.ToIntVector2();
        door.direction = direction;
        door.Initialize();
        if (checkBlock && (!ec.CheckPath(door.aTile, door.bTile, PathType.Nav) || !ec.CheckPath(door.bTile, door.aTile, PathType.Nav)))
        {
            door.UnInitialize();
            UnityEngine.Object.Destroy(door.gameObject);
            return false;
        }

        RendererContainer component = door.GetComponent<RendererContainer>();
        if (component != null)
        {
            ec.CellFromPosition(position).renderers.AddRange(component.renderers);
        }

        cell.HardCoverWall(direction, covered: true);
        cell.HardCoverEntirely();
        if (cell2 != null)
        {
            cell.HardCoverWall(direction.GetOpposite(), covered: true);
        }

        return true;
    }
}