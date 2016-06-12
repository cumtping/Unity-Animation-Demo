using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	bool isDebug = true;
	public GameObject idleBear, idleCat, idleChicken, idleFox, idleFrog, idleHorse;
	public GameObject gameBoard;
	public GameObject gameGrid;
	public GameObject tileSelect;

	GameObject[] idleAnimals;
	string[] clickAnimParam;
	GameObject tileSelectInstance;

	int[,] animalIndexArray = new int[7, 7]{
		{-1, 0, 0, 0, -1, -1, -1},
		{0, 0, 0, 0, 0, -1, -1},
		{0, 0, 0, 0, 0, 0, -1},
		{-1, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, -1},
		{0, 0, 0, 0, 0, -1, -1},
		{-1, 0, 0, 0, -1, -1, -1}
	};
	int xNum = 7, yNum = 7;
	GameObject[,] animalObjArray = new GameObject[7, 7];
	float blockSize = 60;
	float blockGap = 2;
	float screenRadio;
	Vector2 block0Center;
	Vector2 block0StartInWorld;
	Vector2 mouseDownPos;
	Vector2 gameBoardCenter;
	// 上次播放点击动画的位置
	Vector2 lastClickAnimPos = new Vector2(0, 0);

	void Start () {
		init ();
	}
	
	void Update () {
		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}
		dealWithTouchEvent ();
	}

	void init(){
		initData ();
		initAnim ();
		initGameBoard ();
	}

	void initData(){
		// size & gap radio, reference resolution:480x800
		screenRadio = Screen.width / 480;
		// （0, 0）点中心距GameBoard正中心的位置
		int block0CenterX = -1 * (int)((blockSize + blockGap) * 3);
		int block0CenterY = (int)((blockSize + blockGap) * 3);
		block0Center = new Vector2 (block0CenterX, block0CenterY);
		gameBoardCenter = gameBoard.GetComponent<Transform> ().position;
		// 注意：下面要乘以screeRadio
		block0StartInWorld = new Vector2 ((block0CenterX - blockSize / 2) * screenRadio + gameBoardCenter.x,
			(block0CenterY + blockSize / 2) * screenRadio + gameBoardCenter.y);

		idleAnimals = new GameObject[]{idleBear, idleCat, idleChicken, idleFox, idleFrog, idleHorse};
	}

	void initAnim(){
		clickAnimParam = new string[]{"bear_click_start", "cat_click_start", "chicken_click_start",
			"fox_click_start", "frog_click_start", "horse_click_start"};
	}

	void initGameBoard(){
		initGameGrid ();
		createAnimals ();
	}

	void initGameGrid(){
		// TODO 动态隐藏方格
	}

	void dealWithTouchEvent (){
		if (Input.GetMouseButtonDown (0)) {
			mouseDownPos = Input.mousePosition;
		} else if (Input.GetMouseButtonUp(0)) {
			Vector2 upPos = Input.mousePosition;
			float moveX = upPos.x - mouseDownPos.x;
			float moveY = upPos.y - mouseDownPos.y;
			if (Mathf.Abs (moveX) > 10 || Mathf.Abs (moveY) > 10) {
				dealWithMove (moveX, moveY);
			} else {
				dealWithClick (upPos.x, upPos.y);
			}
		}
	}

	void dealWithMove(float moveX, float moveY){
		if (isDebug) Debug.Log ("========= Move START===================================");
		if (Mathf.Abs (moveX) > Mathf.Abs (moveY)) {
			if (moveX > 0) {
				if(isDebug) Debug.Log ("Right");
			} else {
				if(isDebug) Debug.Log ("Left");
			}
		} else {
			if (moveY > 0) {
				if(isDebug) Debug.Log ("UP");
			} else {
				if(isDebug) Debug.Log ("Down");
			}
		}
		if (isDebug) Debug.Log ("=========Move END===================================");
	}

	void dealWithClick(float xPos, float yPos){
		Vector2 clickPos = posToIndex (xPos, yPos);
		int x = (int)clickPos.x;
		int y = (int)clickPos.y;
		if (isDebug) Debug.Log ("click[" + x + ", " + y + "]");

		if (x < 0 || y < 0 || x >= xNum || y >= yNum) {
			if (isDebug) Debug.Log ("Error!!! index out of bound!!");
			return;
		}

		toggleClickAnim ((int)lastClickAnimPos.x, (int)lastClickAnimPos.y, false);
		toggleClickAnim (x, y, true);
	}

	void toggleClickAnim(int x, int y, bool play){
		int animIndex = animalIndexArray [x, y];
		if (animIndex > 0) {
			Animator anim = animalObjArray[x, y].GetComponent<Animator>();
			if (anim != null) {
				anim.SetBool (clickAnimParam[animIndex - 1], play);
				if (play) {
					lastClickAnimPos = new Vector2 (x, y);

					// 边框动画
					createTileSelectAtPos (x, y);
				}
			}
		}
	}

	// 创建选中边框动画
	void createTileSelectAtPos(int x, int y){
		if (tileSelectInstance != null) {
			DestroyObject (tileSelectInstance);
		}

		Vector2 pos = getBlockPos (x, y);

		tileSelectInstance = (GameObject)Instantiate (tileSelect, 
			new Vector3(pos.x, pos.y, 0), Quaternion.identity); 
		tileSelectInstance.transform.SetParent(gameBoard.transform, false);

		Animator anim = tileSelectInstance.GetComponent<Animator> ();
		if (anim != null) {
			anim.SetBool ("tile_select_start", true);
		}
	}
	// 世界坐标与位置的对应
	Vector2 posToIndex(float xPos, float yPos){
		float x = xPos - block0StartInWorld.x;
		float y = block0StartInWorld.y - yPos;
		// 注意：下面要除以screeRadio
		int xIndex = (int)(x / (blockSize + blockGap) / screenRadio);
		int yIndex = (int)(y / (blockSize + blockGap) / screenRadio);
		return new Vector2 (xIndex, yIndex);
	}

	// 创建小动物
	void createAnimals(){
		for (int i = 0; i < xNum; i++) {
			for (int j = 0; j < yNum; j++) {
				if (animalIndexArray [i, j] != -1) {
					int index = Random.Range (1, idleAnimals.Length);
					createAnimalAtPosition (i, j, index);
					animalIndexArray [i, j] = index;
				}
			}
		}
	}

	// 创建一个小动物
	void createAnimalAtPosition(int x, int y, int animalIndex){
		if (animalIndex < 1 || animalIndex > idleAnimals.Length) {
			Debug.Log ("Error!!! animalIndex out of bound");
			return;
		}
		if (x < 0 || y < 0 || x >= xNum || y >= yNum) {
			Debug.Log ("Error!!! position out of bound");
			return;
		}
		Vector2 pos = getBlockPos (x, y);
		GameObject child = (GameObject)Instantiate (idleAnimals[animalIndex - 1], 
			new Vector3(pos.x, pos.y, 0), Quaternion.identity); 
		child.transform.SetParent(gameBoard.transform, false);
		animalObjArray [x, y] = child;
		animalIndexArray [x, y] = animalIndex;
	}

	// 获得某个block在游戏面板中的坐标位置
	Vector2 getBlockPos(int x, int y) {
		float xPos = block0Center.x + x * (blockSize + blockGap);
		float yPos = block0Center.y - y * (blockSize + blockGap);

		return new Vector2 (xPos, yPos);
	}
}
