using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    [Header("Game Status")]
    // 0 - Not started 1 - ready 2 - checking 3 - running
    [SerializeField]private int GameStatus = 0;
    [SerializeField]private int Time = 0;

    // 0: 中立 1: R-Hero 2: R-Engineer 3/4/5: R-Infantry 6: R-Air 7: R-Sentry 9: R-Lidar 18: R-Outpost 19: R-Base 21: B-Hero 22: B-Engineer 23/24/25: B-Infantry 26: B-Air 27: B-Sentry 29: B-Lidar 38: B-Outpost 39: B-Base;
    public Dictionary<int, RefereeController> RefereeControllerList = new Dictionary<int, RefereeController>();

    private void OnEnable()
    {
        RefereeController.OnDamage += DamageUpload;
        RefereeController.OnShoot += ShootUpload;
        RefereeController.OnOccupy += OccupyUpload;
        RefereeController.OnSpawn += SpawnUpload;
    }

    private void OnDisable()
    {
        RefereeController.OnDamage -= DamageUpload;
        RefereeController.OnShoot -= ShootUpload;
        RefereeController.OnOccupy -= OccupyUpload;
        RefereeController.OnSpawn -= SpawnUpload;
    }

    private void Start() {
        // TODO: Need to register new referee controller dynamicallyRefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        
        // TODO: Initial the default RobotStatusList according to a config file;
        Debug.Log(JSONReader.LoadResourceTextfile("RMUC2023/RobotConfig.json"));

        RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None); 
        Debug.Log(_list.Length);
        foreach(RefereeController _referee in _list)
        {
            RefereeControllerList.Add(_referee.RobotID, _referee);

            switch(_referee.RobotID){
                case 3:
                case 4:
                case 5:
                    RefereeControllerList[_referee.RobotID].SetHP(200);
                    break;
                case 18:
                case 38:
                    RefereeControllerList[_referee.RobotID].SetHP(1500);
                    break;
                case 19:
                case 39:
                    RefereeControllerList[_referee.RobotID].SetHP(5000);
                    break;
                default:
                    RefereeControllerList[_referee.RobotID].SetHP(500);
                    break;
            }

            Debug.Log("[GameController] _referee: " + _referee.gameObject.name + " " + _referee.RobotID);
            Debug.Log("[GameController] _referee: " + RefereeControllerList[_referee.RobotID].GetHP());
        }
    }


    /**
    * These status need to be updated on fixed interval
    * 
    */

    void Update()
    {
        // TODO: Update Status Struct according RobotID to every RefreeController

        Time += 1;
        // Debug.Log("[GameController] HP: "+ RobotStatusList[18].GetHP());
    }

    /**
    * The following part is the handler for every events caught during the game
    * 0. Spawn Event -> Refree Controller
    * 1. Damage Event -> Armor Controller
    * 2. Shoot Event -> Shooter Controller
    * 3. Occupy Event -> RFID Controller
    * 4. Purchase Event -> Supply Controller
    * 5. Exchange Event -> Exchange Controller
    * 6. Warning Event -> Judge Controller
    */

    void SpawnUpload(int robotID)
    {
        Debug.Log($"[GameManager] Referee {robotID} SpawnUpload");
        SpawnRefereeServerRpc(robotID);
    }

    [ServerRpc]
    void SpawnRefereeServerRpc(int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"[GameManager] Referee {robotID} ServerRPC");
        if (RefereeControllerList.ContainsKey(robotID))
        {
            Debug.LogError($"{robotID} exists !!!");
            return;
        }

        RefereeController[] _list = GameObject.FindObjectsByType<RefereeController>(FindObjectsSortMode.None);
        Debug.Log($"[GameManager] List Length: {_list.Length}");
        foreach (RefereeController _referee in _list)
        {
            if (_referee.RobotID == robotID)
            {
                Debug.Log($"[GameManager] {robotID} referee added to gamemanager");
                RefereeControllerList.Add(_referee.RobotID, _referee);
                return;
            }
        }
    }

    void DamageUpload(int damageType, int armorID, int robotID)
    {
        DamageHandlerServerRpc(damageType, armorID, robotID);
    }

    [ServerRpc]
    void DamageHandlerServerRpc(int damageType, int armorID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Damage Type: " + damageType + " Armor ID: " + armorID + " Robot ID: " + robotID);
        // Not Disabled or Immutable
        if (!RefereeControllerList[robotID].GetDisabled() && !RefereeControllerList[robotID].GetImmutable()) {
            int _hp = RefereeControllerList[robotID].GetHP();
            int _damage = 0;
            // Debug.Log("[GameManager - Damage] HP:"+RobotStatusList[robotID].HP);
            switch(damageType){
                case 0:
                    _damage = 2;
                    break;
                case 1:
                    _damage = 10;
                    break;
                case 2:
                    _damage = 100;
                    break;
                case 3:
                    _damage = 750;
                    break;
                default:
                    Debug.LogWarning("Unknown Damage Type" + damageType);
                    break;
            }
            RefereeControllerList[robotID].SetHP(_hp - _damage);
        }
    }

    void ShootUpload(int shooterID, int shooterType, int robotID, Vector3 userPosition, Vector3 shootVelocity)
    {
        ShootHandlerServerRpc(shooterID, shooterType, robotID);
    }

    [ServerRpc]
    void ShootHandlerServerRpc(int shooterID, int shooterType, int robotID, ServerRpcParams serverRpcParams = default)
    {
        
    }

    void OccupyUpload(int areaID, int robotID)
    {
        OccupyHandlerServerRpc(areaID, robotID);
    }
    
    [ServerRpc]
    void OccupyHandlerServerRpc(int areaID, int robotID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"[GameManager] {robotID} occupied area {areaID}");
    }
}
