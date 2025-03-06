using BBP_Playables.Core;
using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;
using System;
using UnityEngine;
using System.Collections;
using MTM101BaldAPI.Reflection;
using System.Linq;

namespace BBP_Playables.Modded.BBTimes;

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

    void Update()
    {
        cooldownToShoot -= Time.deltaTime * ec.EnvironmentTimeScale;
        if (cooldownToShoot < 0f)
        {
            cooldownToShoot += UnityEngine.Random.Range(8f, 10f);
            ITM_Basketball iTM_Basketball = GameObject.Instantiate(basketBallPre);
            iTM_Basketball.Setup(ec, transform.forward, transform.position + transform.forward * 7f + Vector3.up * 5f, ec.mainHall, 0.8f);
            iTM_Basketball.ReflectionSetVariable("maxHitsBeforeDying", 1);
            iTM_Basketball.ReflectionSetVariable("lifeTime", 8f);
            for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                iTM_Basketball.GetComponent<Entity>().IgnoreEntity(CoreGameManager.Instance.GetPlayer(i).plm.Entity, true);
            audMan.PlaySingle(audBoom);
        }
    }
}
