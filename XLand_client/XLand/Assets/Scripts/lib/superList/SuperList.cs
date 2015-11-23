﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using System;

namespace xy3d.tstd.lib.superList
{
	public class SuperList : MonoBehaviour
	{
		[SerializeField]
		private bool autoStart = true;
		[SerializeField]
		private bool needSelectedIndex = true;

		[SerializeField]
		private float verticalGap;
		[SerializeField]
		private float horizontalGap;

		[SerializeField]
		private GameObject go;

		private GameObject container;
		private GameObject pool;

		private RectTransform rectTransform;
		private SuperScrollRect scrollRect;

		private float width;
		private float height;
		private float cellWidth;
		private float cellHeight;

		private float cellScale;

		[SerializeField]
		private bool isVertical;

		[SerializeField]
		private bool isRestrain;

		private float containerWidth;
		private float containerHeight;

		private List<object> data = new List<object> ();

		private int rowNum;//最多同时显示多少行
		private int colNum;//最多同时显示多少列

		private List<SuperListCell> showPool = new List<SuperListCell> ();
		private List<SuperListCell> hidePool = new List<SuperListCell> ();

		private int showIndex;
		private int selectedIndex = -1;
		private float curPercent = 0;

		private int[] insertIndexArr;
		private RectTransform[] insertRectArr;
		private float allInsertFix = 0;
		
		public Action<object> CellClickHandle;
		public Action<int> CellClickIndexHandle;

		public void SetData<T> (List<T> _data)
		{
			if(data.Count > 0){

				data.Clear ();
			}

			foreach (T unit in _data) {

				data.Add (unit);
			}

			SetContainerSize();

			showIndex = 0;

			ResetPos (0,true,false);
		}

		private void SetContainerSize(){

			if (isVertical) {
				
				containerHeight = Mathf.Ceil (1.0f * (data.Count - colNum) / colNum) * (cellHeight + verticalGap) + cellHeight;
				
				if (containerHeight < height) {
					
					containerHeight = height;
				}

				if(insertIndexArr != null){

					allInsertFix = 0;

					for(int i = 0 ; i < insertIndexArr.Length ; i++){

						allInsertFix += insertRectArr[i].rect.height + verticalGap;
					}

					containerHeight += allInsertFix;
				}
				
				rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x, 0f);
				rectTransform.offsetMin = new Vector2 (rectTransform.offsetMin.x, -containerHeight);
				rectTransform.offsetMax = new Vector2 (width,0);
				
				curPercent = 1;
				
			} else {
				
				containerWidth = Mathf.Ceil (1.0f * (data.Count - rowNum) / rowNum) * (cellWidth + horizontalGap) + cellWidth;
				
				if (containerWidth < width) {
					
					containerWidth = width;
				}

				if(insertIndexArr != null){
					
					allInsertFix = 0;

					for(int i = 0 ; i < insertIndexArr.Length ; i++){
						
						allInsertFix += insertRectArr[i].rect.width + horizontalGap;
					}
					
					containerWidth += allInsertFix;
				}
				
				rectTransform.anchoredPosition = new Vector2 (0f, rectTransform.anchoredPosition.y);
				rectTransform.offsetMax = new Vector2 (containerWidth, rectTransform.offsetMax.y);
				rectTransform.offsetMin = new Vector2 (0,-height);
				
				curPercent = 0;
			}
		}

		public void SetInsert (int[] _indexs, RectTransform[] _rects){

			insertIndexArr = _indexs;

			insertRectArr = _rects;

			SetContainerSize();

			float fix = 0;

			for(int i = 0 ; i < insertIndexArr.Length ; i++){

				_rects[i].SetParent(container.transform,false);

				if(isVertical){

					insertRectArr[i].anchoredPosition = new Vector2(0,-(cellHeight + verticalGap) * insertIndexArr[i] - fix);

					fix += _rects[i].rect.height + verticalGap;

				}else{

					insertRectArr[i].anchoredPosition = new Vector2((cellWidth + horizontalGap) * insertIndexArr[i] + fix,0);
					
					fix += _rects[i].rect.width + horizontalGap;
				}
			}
			
			showIndex = 0;

			ResetPos(0,false,true);
		}

		public void SetDataAndKeepLocation<T> (List<T> _data)
		{
			float percent = curPercent;

			SetData<T> (_data);

			curPercent = percent;

			SetCurPercent ();
		}

		public void Clear ()
		{
			data.Clear ();

			SetData (data);
		}

		private void SetSize ()
		{
			if (isVertical) {

				float tmpCellWidth = (go.transform as RectTransform).rect.width;

				if (width >= tmpCellWidth) {

					colNum = (int)Mathf.Floor ((width - tmpCellWidth) / (tmpCellWidth + horizontalGap)) + 1;

					cellScale = 1;

					cellWidth = tmpCellWidth;
					cellHeight = (go.transform as RectTransform).rect.height;

				} else {

					colNum = 1;

					cellScale = width / tmpCellWidth;

					cellWidth = width;
					cellHeight = (go.transform as RectTransform).rect.height * cellScale;
				}

				int n = (int)(height / (cellHeight + verticalGap));
				
				if (height - n * (cellHeight + verticalGap) < verticalGap) {
					
					rowNum = n + 1;
					
				} else {
					
					rowNum = n + 2;
				}

			} else {

				float tmpCellHeight = (go.transform as RectTransform).rect.height;

				if (height >= tmpCellHeight) {

					rowNum = (int)Mathf.Floor ((height - tmpCellHeight) / (tmpCellHeight + verticalGap)) + 1;

					cellScale = 1;

					cellWidth = (go.transform as RectTransform).rect.width;
					cellHeight = tmpCellHeight;

				} else {

					rowNum = 1;
					
					cellScale = height / tmpCellHeight;
					
					cellWidth = (go.transform as RectTransform).rect.width * cellScale;
					cellHeight = height;
				}

				int n = (int)(width / (cellHeight + verticalGap));
				
				if (width - n * (cellWidth + horizontalGap) < horizontalGap) {
					
					colNum = n + 1;
					
				} else {
					
					colNum = n + 2;
				}
			}
		}

		void CreateCells ()
		{
			for (int i = 0; i < rowNum * colNum; i++) {
				
				GameObject unit = GameObject.Instantiate (go);

				unit.name = unit.name + i;
				
				unit.transform.SetParent (pool.transform, false);

				(unit.transform as RectTransform).localScale = new Vector3 (cellScale, cellScale, cellScale);
				
				(unit.transform as RectTransform).pivot = new Vector2 (0, 1);
				
				(unit.transform as RectTransform).anchorMin = new Vector2 (0, 1);
				
				(unit.transform as RectTransform).anchorMax = new Vector2 (0, 1);

				hidePool.Add (unit.GetComponent<SuperListCell>());
			}
		}

		// Use this for initialization
		void Awake ()
		{
			scrollRect = gameObject.AddComponent<SuperScrollRect> ();

			scrollRect.vertical = isVertical;

			scrollRect.horizontal = !isVertical;

			scrollRect.isRestrain = isRestrain;

			gameObject.AddComponent<RectMask2D>();

			Image img = gameObject.AddComponent<Image>();

			img.material = new Material(Shader.Find("Custom/UI/Default"));

			img.color = Color.clear;

//			Mask mask = gameObject.AddComponent<Mask>();
//
//			mask.showMaskGraphic = false;
//
//			gameObject.AddComponent<Image>();

			if (autoStart) {

				Init ();
			}
		}

		public void Init ()
		{
			container = new GameObject ("Container", typeof(RectTransform));

			container.transform.SetParent (transform, false);
			
			pool = new GameObject ("Pool", typeof(RectTransform));
			
			pool.transform.SetParent (transform, false);
			
			pool.SetActive (false);
			
			rectTransform = container.transform as RectTransform;
			
			rectTransform.anchorMin = new Vector2 (0, 1);
			rectTransform.anchorMax = new Vector2 (0, 1);

			rectTransform.pivot = new Vector2 (0, 1);
			
			rectTransform.offsetMin = new Vector2 ();
			rectTransform.offsetMax = new Vector2 ();

			scrollRect.content = rectTransform;
			
			width = (transform as RectTransform).rect.width;
			height = (transform as RectTransform).rect.height;

			scrollRect.onValueChanged.AddListener (new UnityEngine.Events.UnityAction<Vector2> (OnScroll));

			SetSize ();
			
			CreateCells ();
		}

		void OnScroll (Vector2 _v)
		{
			int nowIndex;

			if (isVertical) {

				curPercent = _v.y;

				float y;

				if (curPercent < 0) {
					
					y = 1;
					
				} else if (curPercent > 1) {
					
					y = 0;
					
				} else {
					
					y = 1 - curPercent;
				}

				if(insertIndexArr == null){

					nowIndex = (int)(y * (containerHeight - height) / (cellHeight + verticalGap)) * colNum;

				}else{

					FixY(0,y,ref y,0);
					
					nowIndex = (int)(y * (containerHeight - height - allInsertFix) / (cellHeight + verticalGap)) * colNum;
				}

			} else {

				curPercent = _v.x;

				float x;

				if (curPercent < 0) {
					
					x = 0;
					
				} else if (curPercent > 1) {
					
					x = 1;
					
				} else {
					
					x = curPercent;
				}

				if(insertIndexArr == null){

					nowIndex = (int)(x * (containerWidth - width) / (cellWidth + horizontalGap)) * rowNum;

				}else{
					
					FixX(0,x,ref x,0);
					
					nowIndex = (int)(x * (containerWidth - width - allInsertFix) / (cellWidth + horizontalGap)) * rowNum;
				}
			}

			if (nowIndex != showIndex) {

				ResetPos (nowIndex,false,false);
			}
		}

		private void FixY(int _startIndex,float _yOld,ref float _y,float _fix){

			int tmpIndex = (int)(_y * (containerHeight - height - allInsertFix) / (cellHeight + verticalGap));

			if(tmpIndex > insertIndexArr[_startIndex]){

				_fix += insertRectArr[_startIndex].rect.height + verticalGap;
					
				_y = ((containerHeight - height - allInsertFix + _fix) * _yOld - _fix) / (containerHeight - height - allInsertFix);

				if(_startIndex < insertIndexArr.Length - 1){

					FixY(_startIndex + 1,_yOld,ref _y,_fix);
				}
			}
		}

		private void FixX(int _startIndex,float _xOld,ref float _x,float _fix){
			
			int tmpIndex = (int)(_x * (containerWidth - width - allInsertFix) / (cellWidth + horizontalGap));

			if(tmpIndex > insertIndexArr[_startIndex]){
				
				_fix += insertRectArr[_startIndex].rect.width + horizontalGap;
				
				_x = ((containerWidth - width - allInsertFix + _fix) * _xOld - _fix) / (containerWidth - width - allInsertFix);

				if(_startIndex < insertIndexArr.Length - 1){

					FixX(_startIndex + 1,_xOld,ref _x,_fix);
				}
			}
		}

		private void ResetPos (int _nowIndex,bool _dataHasChange,bool _insertHasChange)
		{
//			if (data.Count == 0) {
//
//				return;
//			}

			if (isVertical) {

				if (_nowIndex - showIndex == colNum) {

					int row = _nowIndex / colNum + (rowNum - 1);

					for (int i = 0; i < _nowIndex - showIndex; i++) {

						int newIndex = showIndex + rowNum * colNum + i;

						SuperListCell unit = showPool [0];

						showPool.RemoveAt (0);

						int col = i;

						if (newIndex < data.Count) {

							showPool.Add (unit);

							SetCellIndex (unit, newIndex);

							SetCellData (unit, newIndex);

						} else {

							hidePool.Add (unit);

							unit.transform.SetParent (pool.transform, false);
						}
					}

				} else if (_nowIndex - showIndex == -colNum) {

					int row = _nowIndex / colNum;
				
					for (int i = 0; i < showIndex - _nowIndex; i++) {
					
						int newIndex = showIndex - colNum + i;

						SuperListCell unit;

						if (showPool.Count == rowNum * colNum) {

							unit = showPool [showPool.Count - 1];

							showPool.RemoveAt (showPool.Count - 1);

						} else {

							unit = hidePool [0];

							hidePool.RemoveAt (0);

							unit.transform.SetParent (container.transform, false);

							//因为现在Rect2DMask又bug  所以加了这2句2B代码尝试拯救一下
							unit.gameObject.SetActive(false);

							unit.gameObject.SetActive(true);
						}

						showPool.Insert (0, unit);

						int col = i;

						SetCellIndex (unit, newIndex);

						SetCellData (unit, newIndex);
					}

				} else {

					List<int> tmpList = new List<int> ();

					for (int i = 0; i < rowNum * colNum; i++) {

						if (_nowIndex + i < data.Count) {

							tmpList.Add (_nowIndex + i);

						} else {

							break;
						}
					}

					SuperListCell[] newShowPool = new SuperListCell[tmpList.Count];

					List<SuperListCell> replacePool = new List<SuperListCell> ();

					for (int i = 0; i < showPool.Count; i++) {

						SuperListCell unit = showPool [i];

						int tmpIndex = unit.index;

						if (tmpList.Contains (tmpIndex)) {

							newShowPool [tmpList.IndexOf (tmpIndex)] = unit;

							if(_dataHasChange){

								SetCellData(unit,tmpIndex);//如果列表数据已经发生了变化则需要在这里进行一次单元格赋值
							}

							if(_insertHasChange){

								SetCellIndex(unit,tmpIndex);
							}

						} else {

							replacePool.Add (unit);
						}
					}

					showPool.Clear ();

					for (int i = 0; i < newShowPool.Length; i++) {

						if (newShowPool [i] == null) {

							SuperListCell unit;

							if (replacePool.Count > 0) {

								unit = replacePool [0];

								replacePool.RemoveAt (0);

							} else {

								unit = hidePool [0];

								hidePool.RemoveAt (0);

								unit.transform.SetParent (container.transform, false);

								//因为现在Rect2DMask又bug  所以加了这2句2B代码尝试拯救一下
								unit.gameObject.SetActive(false);
								
								unit.gameObject.SetActive(true);
							}

							newShowPool [i] = unit;

							SetCellIndex (unit, tmpList [i]);

							SetCellData (unit, tmpList [i]);
						}

						showPool.Add (newShowPool [i]);
					}

					foreach (SuperListCell unit in replacePool) {

						unit.transform.SetParent (pool.transform, false);

						hidePool.Add (unit);
					}
				}

			} else {

				if (_nowIndex - showIndex == rowNum) {
					
					int col = _nowIndex / rowNum + (colNum - 1);
					
					for (int i = 0; i < _nowIndex - showIndex; i++) {
						
						int newIndex = showIndex + rowNum * colNum + i;
						
						SuperListCell unit = showPool [0];

						showPool.RemoveAt (0);
						
						int row = i;
						
						if (newIndex < data.Count) {
							
							showPool.Add (unit);
							
							SetCellIndex (unit, newIndex);

							SetCellData (unit, newIndex);

						} else {
							
							hidePool.Add (unit);
							
							unit.transform.SetParent (pool.transform, false);
						}
					}
					
				} else if (_nowIndex - showIndex == -rowNum) {
					
					int col = _nowIndex / rowNum;
					
					for (int i = 0; i < showIndex - _nowIndex; i++) {
						
						int newIndex = showIndex - rowNum + i;
						
						SuperListCell unit;
						
						if (showPool.Count == rowNum * colNum) {
							
							unit = showPool [showPool.Count - 1];
							
							showPool.RemoveAt (showPool.Count - 1);
							
						} else {
							
							unit = hidePool [0];
							
							hidePool.RemoveAt (0);
							
							unit.transform.SetParent (container.transform, false);

							//因为现在Rect2DMask又bug  所以加了这2句2B代码尝试拯救一下
							unit.gameObject.SetActive(false);
							
							unit.gameObject.SetActive(true);
						}
						
						showPool.Insert (0, unit);
						
						int row = i;
						
						SetCellIndex (unit, newIndex);

						SetCellData (unit, newIndex);
					}
					
				} else {
					
					List<int> tmpList = new List<int> ();
					
					for (int i = 0; i < rowNum * colNum; i++) {
						
						if (_nowIndex + i < data.Count) {
							
							tmpList.Add (_nowIndex + i);
							
						} else {
							
							break;
						}
					}
					
					SuperListCell[] newShowPool = new SuperListCell[tmpList.Count];
					
					List<SuperListCell> replacePool = new List<SuperListCell> ();
					
					for (int i = 0; i < showPool.Count; i++) {
						
						SuperListCell unit = showPool [i];
						
						int tmpIndex = unit.index;
						
						if (tmpList.Contains (tmpIndex)) {
							
							newShowPool [tmpList.IndexOf (tmpIndex)] = unit;

							if(_dataHasChange){

								SetCellData(unit,tmpIndex);
							}

							if(_insertHasChange){
								
								SetCellIndex(unit,tmpIndex);
							}
							
						} else {
							
							replacePool.Add (unit);
						}
					}
					
					showPool.Clear ();
					
					for (int i = 0; i < newShowPool.Length; i++) {
						
						if (newShowPool [i] == null) {
							
							SuperListCell unit;
							
							if (replacePool.Count > 0) {
								
								unit = replacePool [0];
								
								replacePool.RemoveAt (0);
								
							} else {
								
								unit = hidePool [0];
								
								hidePool.RemoveAt (0);
								
								unit.transform.SetParent (container.transform, false);

								//因为现在Rect2DMask又bug  所以加了这2句2B代码尝试拯救一下
								unit.gameObject.SetActive(false);
								
								unit.gameObject.SetActive(true);
							}
							
							newShowPool [i] = unit;

							SetCellIndex (unit, tmpList [i]);

							SetCellData (unit, tmpList [i]);
						}	
						
						showPool.Add (newShowPool [i]);
					}
					
					foreach (SuperListCell unit in replacePool) {
						
						unit.transform.SetParent (pool.transform, false);
						
						hidePool.Add (unit);
					}
				}
			}

			showIndex = _nowIndex;
		}

		private void SetCellIndex (SuperListCell _cell, int _index)
		{
			int row;

			int col;

			float xFix = 0;

			float yFix = 0;

			if(isVertical){

				row = _index / colNum;
				
				col = _index % colNum;

				if(insertIndexArr != null){

					for(int i = 0 ; i < insertIndexArr.Length ; i++){

						if(row >= insertIndexArr[i]){

							yFix += insertRectArr[i].rect.height + verticalGap;

						}else{

							break;
						}
					}
				}

			}else{

				col = _index / rowNum;
				
				row = _index % rowNum;

				if(insertIndexArr != null){
					
					for(int i = 0 ; i < insertIndexArr.Length ; i++){
						
						if(col >= insertIndexArr[i]){

							xFix += insertRectArr[i].rect.width + horizontalGap;
							
						}else{
							
							break;
						}
					}
				}
			}

			float xPos = col * (cellWidth + horizontalGap) + xFix;

			float yPos = -row * (cellHeight + verticalGap) - yFix;

			Vector2 pos = new Vector2(xPos, yPos);

			(_cell.transform as RectTransform).anchoredPosition = pos;
			
			_cell.index = _index;

			_cell.SetSelected (_index == selectedIndex);
		}
	
		private void SetCellData(SuperListCell _cell,int _index){

			_cell.SetData (data[_index]);
		}

		void CellClick (SuperListCell _cell)
		{
			SetSelectedIndex (_cell.index);
		}

		public void SetSelectedIndex (int _index)
		{
			if (selectedIndex == _index) {

				return;
			}

			if (needSelectedIndex) {

				foreach (SuperListCell cell in showPool) {

					if (cell.index == _index) {
						
						cell.SetSelected (true);
					}
					else if (cell.index == selectedIndex) {

						cell.SetSelected (false);
				
					}
				}

				selectedIndex = _index;
			}

			if (selectedIndex != -1 || !needSelectedIndex) {

				if (CellClickHandle != null) {
					
					CellClickHandle (data [_index]);
				}

				if (CellClickIndexHandle != null) {

					CellClickIndexHandle (_index);
				}
			}

		}

		public int GetSelectedIndex ()
		{
			return selectedIndex;
		}

		public void ResetSelectIndex()
		{
			selectedIndex = -1;
		}

		public void DisplayIndex (int _index)
		{
			float finalPos;

			if (isVertical) {

				if (containerHeight > height) {

					float pos = (_index / colNum) * (cellHeight + verticalGap);

					finalPos = 1 - pos / (containerHeight - height);

					if (finalPos < 0) {

						finalPos = 0;

					} else if (finalPos > 1) {

						finalPos = 1;
					}

				} else {

					finalPos = 1;
				}

			} else {

				if (containerWidth > width) {

					float pos = (_index / rowNum) * (cellWidth + horizontalGap);

					finalPos = pos / (containerWidth - width);

					if (finalPos < 0) {
						
						finalPos = 0;

					} else if (finalPos > 1) {

						finalPos = 1;
					}

				} else {

					finalPos = 0;
				}
			}

			curPercent = finalPos;

			SetCurPercent();
		}

		public void UpdateItemAt (int _index, object _data)
		{
			data [_index] = _data;

			foreach (SuperListCell cell in showPool) {

				if (cell.index == _index) {
					
					cell.SetData (_data);

					break;
				}
			}
		}

		public object GetDataByIndex(int _index)
		{
			return data[_index];
		}

		private void SetCurPercent ()
		{
			try{

				if (isVertical) {
					
					scrollRect.verticalNormalizedPosition = curPercent;

					OnScroll(new Vector2(0,curPercent));
					
				} else {
					
					scrollRect.horizontalNormalizedPosition = curPercent;

					OnScroll(new Vector2(curPercent,0));
				}

			}catch(Exception e){

				SuperDebug.Log("SuperList Error!");
			}
		}

		public void StopMovement(){

			scrollRect.StopMovement();
		}



	}
}
