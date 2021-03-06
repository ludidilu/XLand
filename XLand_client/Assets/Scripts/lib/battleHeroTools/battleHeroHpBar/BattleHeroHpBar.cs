﻿using UnityEngine;
using System;
using xy3d.tstd.lib.gameObjectFactory;

namespace xy3d.tstd.lib.battleHeroTools
{
    public class BattleHeroHpBar
    {
        public const float boardWidth = 119.0f / 100;
        public const float boardHeight = 43.0f / 100;

        public const float hpBarWidth = 54.0f / 100;
        public const float hpBarHeight = 8.0f / 100;

        public const float hpBarX = 37.0f / 100;
        public const float hpBarY = 14.0f / 100;

        public const float myHpBarU = 147.0f / 100;
        public const float myHpBarV = 1.0f / 100;

        public const float oppHpBarU = 147.0f / 100;
        public const float oppHpBarV = 14.0f / 100;

        public const float iconBarWidth = 40.0f / 100;
        public const float iconBarHeight = 40.0f / 100;

        public const float iconBarX = 0.0f / 100;
        public const float iconBarY = 0.0f / 100;

        public const float iconBarV = 50.0f / 100;

        public const float iconBarXGap = 1.0f / 100;
        public const float iconBarYGap = 1.0f / 100;

        public const float iconBarNumFirstLine = 6.0f;

        public const float angerBarWidth = 54.0f / 100;
        public const float angerBarHeight = 6.0f / 100;

        public const float angerBarX = 37.0f / 100;
        public const float angerBarY = 23.0f / 100;

        public const float angerBarU = 147.0f / 100;
        public const float angerBarV = 27.0f / 100;

        public const float TEXTURE_WIDTH = 256.0f / 100;
        public const float TEXTURE_HEIGHT = 128.0f / 100;

        private MeshRenderer mr;
        private Mesh mesh;

        public const int hpBarNum = 20;
        public const int planeNum = 4;
        private BattleHeroHpBarUnit[] unitVec;

        private GameObject hpBarGO;
        private GameObject container;

        public bool showForce;

        private static BattleHeroHpBar _Instance;

        public static BattleHeroHpBar Instance
        {

            get
            {

                if (_Instance == null)
                {

                    _Instance = new BattleHeroHpBar();
                }

                return _Instance;
            }
        }

        public BattleHeroHpBar()
        {
        }

        public void Init(GameObject con, Action call)
        {

            container = con;

            if (hpBarGO)
            {
                hpBarGO.SetActive(true);
                if (call != null)
                {
                    call();
                }
                return;
            }


            unitVec = new BattleHeroHpBarUnit[hpBarNum];

            for (int i = 0; i < hpBarNum; i++)
            {
                unitVec[i] = new BattleHeroHpBarUnit(i);
            }

            Action<GameObject> loadGameObject = delegate(GameObject _go)
            {
                hpBarGO = _go;
                hpBarGO.transform.SetParent(container.transform, false);

                mr = _go.GetComponent<MeshRenderer>();
                mesh = _go.GetComponent<MeshFilter>().mesh;

                if (call != null)
                {
                    call();
                }
            };

            GameObjectFactory.Instance.GetGameObject("Assets/Arts/battle/BattleTool/HpBar.prefab", loadGameObject, true); 

        }

        public void Update()
        {
			if(mr != null){

	            for (int i = 0; i < hpBarNum; i++)
	            {
	                BattleHeroHpBarUnit unit = unitVec[i];

	                if (unit.IsChange || unit.State == 1)
	                {
	                    if (unit.IsChange) unit.IsChange = false;

						unit.GetPositionsVec(mr.material);

						unit.GetFixVec(mr.material);

						unit.GetStateInfoVec(mr.material);

						unit.GetScaleFix(mr.material);
	                }
	            }
			}
        }

        public BattleHeroHpBarUnit getHpBar(bool _myControl, int _type, float _nowhp, float _maxHp, int _nowAnger, GameObject _go)
        {
            BattleHeroHpBarUnit unit = null;

            for (int i = 0; i < hpBarNum; i++)
            {

                if (unitVec[i].State == 0)
                {

                    unit = unitVec[i];
                    unit.Init(_nowhp, _maxHp, _nowAnger, _go);

                    unit.State = 1;

                    Vector2[] uvs = mesh.uv;
                    if (_myControl)
                    {

                        uvs[i * 16 + 4] = new Vector2(myHpBarU / TEXTURE_WIDTH, 1 - (myHpBarV + hpBarHeight) / TEXTURE_HEIGHT);
                        uvs[i * 16 + 5] = new Vector2((myHpBarU + hpBarWidth) / TEXTURE_WIDTH, 1 - myHpBarV / TEXTURE_HEIGHT);
                        uvs[i * 16 + 6] = new Vector2((myHpBarU + hpBarWidth) / TEXTURE_WIDTH, 1 - (myHpBarV + hpBarHeight) / TEXTURE_HEIGHT);
                        uvs[i * 16 + 7] = new Vector2(myHpBarU / TEXTURE_WIDTH, 1 - myHpBarV / TEXTURE_HEIGHT);

                    }
                    else
                    {

                        uvs[i * 16 + 4] = new Vector2(oppHpBarU / TEXTURE_WIDTH, 1 - (oppHpBarV + hpBarHeight) / TEXTURE_HEIGHT);
                        uvs[i * 16 + 5] = new Vector2((oppHpBarU + hpBarWidth) / TEXTURE_WIDTH, 1 - oppHpBarV / TEXTURE_HEIGHT);
                        uvs[i * 16 + 6] = new Vector2((oppHpBarU + hpBarWidth) / TEXTURE_WIDTH, 1 - (oppHpBarV + hpBarHeight) / TEXTURE_HEIGHT);
                        uvs[i * 16 + 7] = new Vector2(oppHpBarU / TEXTURE_WIDTH, 1 - oppHpBarV / TEXTURE_HEIGHT);

                    }
                    float clanbar_u_fix;
                    float clanbar_v_fix;
                    if (_type < iconBarNumFirstLine + 1)
                    {

                        clanbar_u_fix = (_type - 1) * (iconBarWidth + iconBarXGap);
                        clanbar_v_fix = 0;

                    }
                    else
                    {

                        clanbar_u_fix = (_type - 1) % iconBarNumFirstLine * (iconBarWidth + iconBarXGap);
                        clanbar_v_fix = iconBarHeight + iconBarYGap;
                    }

                    uvs[i * 16 + 8] = new Vector2(clanbar_u_fix / TEXTURE_WIDTH, 1 - (clanbar_v_fix + iconBarHeight + iconBarV) / TEXTURE_HEIGHT);
                    uvs[i * 16 + 9] = new Vector2((clanbar_u_fix + iconBarWidth) / TEXTURE_WIDTH, 1 - (clanbar_v_fix + iconBarV) / TEXTURE_HEIGHT);
                    uvs[i * 16 + 10] = new Vector2((clanbar_u_fix + iconBarWidth) / TEXTURE_WIDTH, 1 - (clanbar_v_fix + iconBarHeight + iconBarV) / TEXTURE_HEIGHT);
                    uvs[i * 16 + 11] = new Vector2(clanbar_u_fix / TEXTURE_WIDTH, 1 - (clanbar_v_fix + iconBarV) / TEXTURE_HEIGHT);
                    mesh.uv = uvs;

                    break;
                }
            }

            return unit;
        }

        public void DelHpBar(BattleHeroHpBarUnit _unit)
        {

            _unit.Alpha = 0;
            _unit.State = 0;
            _unit.IsChange = true;

        }


        public void Dispose()
        {
            hpBarGO.SetActive(false);
        }


    }
}
