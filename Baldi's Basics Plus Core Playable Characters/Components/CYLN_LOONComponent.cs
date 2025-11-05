using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBP_Playables.Core
{
    public class CYLN_LOONComponent : PlayableCharacterComponent
    {
        public override void SpoopBegin(BaseGameManager manager) => StartCoroutine(CylnObjects(manager));
        public static readonly List<GameObject> prefabs = new List<GameObject>();
        internal bool LastChance;

        private float CylnTime() => UnityEngine.Random.Range(60f, 90f);

        private IEnumerator CylnObjects(BaseGameManager manager)
        {
            float time;
            while (manager != null)
            {
                time = CylnTime();
                while (time > 0f)
                {
                    if (manager == null)
                        yield break;
                    time -= manager.Ec.EnvironmentTimeScale * Time.deltaTime;
                    yield return null;
                }
                if (manager == null)
                    yield break;
                List<Cell> hallcells = new List<Cell>();
                for (int i = 0; i < manager.Ec.levelSize.x; i++)
                {
                    for (int j = 0; j < manager.Ec.levelSize.z; j++)
                    {
                        if (manager.Ec.cells[i, j] != null && manager.Ec.cells[i, j].room.category == RoomCategory.Hall
                            && !manager.Ec.cells[i, j].open && !manager.Ec.cells[i, j].HasObjectBase
                            && !Physics.OverlapSphere(manager.Ec.cells[i, j].CenterWorldPosition, 0.5f).ToList().Exists(x => x.gameObject.GetComponent<ThrowableObject>()))
                            hallcells.Add(manager.Ec.cells[i, j]);
                    }
                }
                if (hallcells.Count > 0)
                {
                    GameObject table = GameObject.Instantiate(PlayableCharsPlugin.assetMan.Get<Entity>("CYLN_Throwable").gameObject, hallcells[UnityEngine.Random.Range(0, hallcells.Count)].CenterWorldPosition, default(Quaternion), null);
                    var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
                    table.GetComponentInChildren<MeshFilter>(true).sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                    table.GetComponentInChildren<MeshRenderer>(true).SetMaterialArray(prefab.GetComponent<MeshRenderer>().GetMaterialArray());
                }
                yield return null;
            }
            yield break;
        }
    }
}
