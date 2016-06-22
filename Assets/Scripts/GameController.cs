using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	bool isDebug = true;
	bool testMode = false;
	public GameObject idleBear, idleCat, idleChicken, idleFox, idleFrog, idleHorse;
	public GameObject gameBoard;
	public GameObject gameGrid;
	public GameObject tileSelect;
	public GameObject destroyEffect;
	public GameObject debugText;

	GameObject[] idleAnimals;
	string[] clickAnimParam;
	GameObject tileSelectInstance;
	GameObject selectAnimalObj = null;

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
		if (testMode) {
			createTestAnimals ();
		} else {
			createAnimals ();
		}
		updateDebugToUI ();
	}

	void initGameGrid(){
		// TODO 动态隐藏方格
	}
	/// <测试代码>
	void createTestAnimals () {
		animalIndexArray = new int[7, 7]{
			{-1, -1, -1, -1, -1, -1, -1},
			{-1, -1, -1, -1, -1, -1, -1},
			{-1, -1, -1, -1, -1, -1, -1},
			{-1, -1, -1, -1, -1, -1, -1},
			{-1, -1, -1, -1, -1, -1, -1},
			{-1, -1, -1, -1, -1, -1, -1},
			{-1, -1, -1, -1, -1, -1, -1}};

		createAnimalAtPosition (1, 0, 1);
		createAnimalAtPosition (1, 1, 1);
		createAnimalAtPosition (1, 2, 2);
		createAnimalAtPosition (2, 2, 1);
	}
	/// </测试代码>

	void dealWithTouchEvent (){
		Vector2 upPos = Input.mousePosition;

		if (Input.GetMouseButtonDown (0)) {
			mouseDownPos = Input.mousePosition;
			// 点击事件
			dealWithClick (upPos.x, upPos.y);
		} else if (Input.GetMouseButtonUp(0)) {
			float moveX = upPos.x - mouseDownPos.x;
			float moveY = upPos.y - mouseDownPos.y;
			if (Mathf.Abs (moveX) > 10 || Mathf.Abs (moveY) > 10) {
				dealWithMove (moveX, moveY);
			}
		}
	}

	// 处理滑动事件
	void dealWithMove(float moveX, float moveY){
		//if (isDebug) Debug.Log ("========= Move START===================================");
		if (Mathf.Abs (moveX) > Mathf.Abs (moveY)) {
			if (moveX > 0) {
				if(isDebug) Debug.Log ("Right");
				switchTwoBlocks (2);
			} else {
				if(isDebug) Debug.Log ("Left");
				switchTwoBlocks (0);
			}
		} else {
			if (moveY > 0) {
				if(isDebug) Debug.Log ("UP");
				switchTwoBlocks (1);
			} else {
				if(isDebug) Debug.Log ("Down");
				switchTwoBlocks (3);
			}
		}
		//if (isDebug) Debug.Log ("=========Move END===================================");
	}

	// 处理点击事件
	void dealWithClick(float xPos, float yPos){
		Vector2 clickPos = posToIndex (xPos, yPos);
		int x = (int)clickPos.x;
		int y = (int)clickPos.y;
		if (isDebug) Debug.Log ("click[" + x + ", " + y + "]");

		if (x < 0 || y < 0 || x >= xNum || y >= yNum) {
			if (isDebug) Debug.Log ("Error!!! index out of bound!!");
			return;
		}

		toggleClickAnim (lastClickAnimPos, false);
		toggleClickAnim (clickPos, true);
	}

	// 播放 / 停止点击动画
	void toggleClickAnim(Vector2 pos, bool play){
		int x = (int)pos.x;
		int y = (int)pos.y;
		int animIndex = animalIndexArray [x, y];
		if (animIndex > 0) {
			Animator anim = animalObjArray[x, y].GetComponent<Animator>();
			if (anim != null) {
				anim.SetBool (clickAnimParam[animIndex - 1], play);
				if (play) {
					lastClickAnimPos = new Vector2 (x, y);
					selectAnimalObj = animalObjArray [x, y];

					// 边框动画
					createTileSelectAtPos (x, y);
				} else {
					if (tileSelectInstance != null) {
						DestroyObject (tileSelectInstance);
					}
				}
			}
		}
	}

	// 创建选中边框动画
	void createTileSelectAtPos(int x, int y){
		tileSelectInstance = createGameObjAtPos (tileSelect, x, y);

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
					int index = randomNewAnimalIndexWhenInit(i, j);
					createAnimalAtPosition (i, j, index);
					animalIndexArray [i, j] = index;
				}
			}
		}
	}

	// 初始化游戏时随机生成一个小动物
	int randomNewAnimalIndexWhenInit(int x, int y){
		int index = Random.Range (1, idleAnimals.Length);
		int[,] arr = animalIndexArray;

		if (x < 2 && y < 2) {
			// do nothing
		} else if (y < 2) {
			// 只做竖向检查
			while (index == arr [x - 1, y] && index == arr [x - 2, y]) {
				index = Random.Range (1, idleAnimals.Length);
			}
		} else if (x < 2) {
			// 只做横向检查
			while (index == arr [x, y - 1] && index == arr [x, y - 2]) {
				index = Random.Range (1, idleAnimals.Length);
			}
		} else {
			// 横向、竖向都检查
			while ((index == arr [x - 1, y] && index == arr [x - 2, y]) ||
				(index == arr [x, y - 1] && index == arr [x, y - 2])) {
				index = Random.Range (1, idleAnimals.Length);
			}
		}
		return index;
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

		GameObject child = createGameObjAtPos (idleAnimals[animalIndex - 1], x, y);
		animalObjArray [x, y] = child;
		animalIndexArray [x, y] = animalIndex;
	}

	// 获得某个block在游戏面板中的坐标位置
	Vector2 getBlockPos(int x, int y) {
		float xPos = block0Center.x + x * (blockSize + blockGap);
		float yPos = block0Center.y - y * (blockSize + blockGap);

		return new Vector2 (xPos, yPos);
	}

	// 获得某个block的世界坐标位置
	Vector2 getBlockWorldPos(int x, int y) {
		float xPos = (block0Center.x + x * (blockSize + blockGap)) * screenRadio;
		float yPos = (block0Center.y - y * (blockSize + blockGap)) * screenRadio;
		return new Vector2 (xPos + gameBoardCenter.x, yPos + gameBoardCenter.y);
	}

	// 交换两个方块（选中方块与其上 / 下 / 左 / 右的方块）
	// 左：0，上：1，右：2，下：3
	// 问题：需要在交换动画完成时再去消除，否则消除过程不太流畅
	void switchTwoBlocks(int orientation){
		if (null == selectAnimalObj){
			if (isDebug) Debug.Log ("switchTwoBlocks: selectAnimalObj is null");
			return;
		}
		Vector2 start = lastClickAnimPos;
		Vector2 end = getSwitchEndPos (start, orientation);

		if (canMoveTo (end)) {
			// 交换位置
			switchPos(start, end, true);
			// 停止动画
			if (lastClickAnimPos != null) {
				toggleClickAnim (lastClickAnimPos, false);
			}
		} else {
			if (isDebug) Debug.Log ("switchTwoBlocks: no need to switch");
		}
	}

	// 交换位置
	void switchPos(Vector2 start, Vector2 end, bool clearWhenSwitchFinish){
		Vector2 startPos = getBlockWorldPos ((int)start.x, (int)start.y);
		Vector2 endPos = getBlockWorldPos ((int)end.x, (int)end.y);

		if (animalObjArray [(int)start.x, (int)start.y] == null) {
			if (isDebug) Debug.Log ("switchPos animalObjArray [" + (int)start.x + ", " +  (int)start.y + "] is null");
			return;
		}
		if (animalObjArray [(int)end.x, (int)end.y] == null) {
			if (isDebug) Debug.Log ("switchPos animalObjArray [" + (int)end.x + ", " +  (int)end.y + "] is null");
			return;
		}

		if (clearWhenSwitchFinish) {
			// 移动完调用switchPosFinish方法；
			iTween.MoveTo (animalObjArray [(int)start.x, (int)start.y],
				iTween.Hash ("x", endPos.x, "y", endPos.y, "delay", .2,
					"oncomplete", "switchPosFinish", 
					"oncompleteparams", new Vector2[]{end, start},
					"oncompletetarget", gameObject));
		} else {
			iTween.MoveTo (animalObjArray [(int)start.x, (int)start.y],
				iTween.Hash ("x", endPos.x, "y", endPos.y, "delay", .2));
		}
		iTween.MoveTo (animalObjArray [(int)end.x, (int)end.y],
			iTween.Hash ("y", startPos.y, "x", startPos.x, "delay", .2));
		// 交换坐标
		int record = animalIndexArray [(int)start.x, (int)start.y];
		animalIndexArray [(int)start.x, (int)start.y] = animalIndexArray [(int)end.x, (int)end.y];
		animalIndexArray [(int)end.x, (int)end.y] = record;

		GameObject recordObj = animalObjArray [(int)start.x, (int)start.y];
		animalObjArray [(int)start.x, (int)start.y] = animalObjArray [(int)end.x, (int)end.y];
		animalObjArray [(int)end.x, (int)end.y] = recordObj;
	}

	// 交换动物结束时判断消除
	void switchPosFinish(Vector2[] positions){
		if (null == positions || positions.Length < 2) {
			return;
		}
		// 判断能否消除
		bool isCanClear = canClear();
		// 不能消除，位置换回来
		if (!isCanClear) {
			switchPos (positions[0], positions[1], false);

			updateDebugToUI ();
		}
	}

	Vector2 getSwitchEndPos(Vector2 switchStart, int orientation){
		if (0 == orientation) {
			// 向左
			return new Vector2 (switchStart.x - 1, switchStart.y);
		} else if (1 == orientation) {
			// 向上
			return new Vector2 (switchStart.x, switchStart.y - 1);
		} else if (2 == orientation) {
			// 向右
			return new Vector2 (switchStart.x + 1, switchStart.y);
		} else if (3 == orientation) {
			// 向下
			return new Vector2 (switchStart.x, switchStart.y + 1);
		}
		return Vector2.zero;
	}

	bool canMoveTo(Vector2 pos){
		return pos != Vector2.zero && pos.x >= 0 && pos.y >= 0 &&
			pos.x < xNum && pos.y < yNum &&
			animalIndexArray [(int)pos.x, (int)pos.y] != -1;
	}

	// 判断能否消除
	bool canClear(){
		bool canClear = false;
		for (int i = 0; i < xNum; i++) {
			for (int j = 0; j < yNum; j++) {
				if (animalIndexArray [i, j] != -1) {
					if (i < xNum - 2 && animalIndexArray [i, j] == animalIndexArray [i + 1, j] &&
					   animalIndexArray [i, j] == animalIndexArray [i + 2, j]) {
						// 横向消除
						clearAnimals(new Vector2[]{new Vector2(i, j),
							new Vector2(i + 1, j), new Vector2(i + 2, j)});
						canClear = true;
					}
					if (j < yNum - 2 && animalIndexArray [i, j] == animalIndexArray [i, j + 1] &&
						animalIndexArray [i, j] == animalIndexArray [i, j + 2]) {
						// 竖向消除
						clearAnimals(new Vector2[]{new Vector2(i, j),
							new Vector2(i, j + 1), new Vector2(i, j + 2)});
						canClear = true;
					}
				}
			}
		}
		return canClear;
	}

	// 消除动物
	void clearAnimals(Vector2[] positions){
		if (positions != null && positions.Length > 0) {
			foreach(Vector2 vect in positions){
				GameObject obj = animalObjArray[(int)vect.x, (int)vect.y];
				if (obj != null) {
					// Play destory animation
					createGameObjAtPos (destroyEffect, (int)vect.x, (int)vect.y);
					Destroy (obj);
					animalIndexArray [(int)vect.x, (int)vect.y] = 0;
				}
			}

			fallDown (new int[]{0, 1, 2, 3, 4, 5, 6});
		}
	}

	// 在x,y位置创建游戏对象
	GameObject createGameObjAtPos(GameObject obj, int x, int y){
		Vector2 pos = getBlockPos (x, y);
		GameObject child = (GameObject)Instantiate (obj, 
			new Vector3(pos.x, pos.y, 0), Quaternion.identity); 
		child.transform.SetParent(gameBoard.transform, false);
		return child;
	}

	// 消除后向下移动
	void fallDown(int[] cols){
		if (cols.Length > 0) {
			foreach(int col in cols){
				oneColFallDown (col);
			}
		}
	}

	// 某一行向下移动
	void oneColFallDown(int col){
		if (isDebug) Debug.Log ("fall down col " + col);
		bool fallDownFinishCalled = false;

		for (int i = yNum - 2; i >= 0; i--) {
			if (animalIndexArray [col, i] != -1 && animalIndexArray [col, i] != 0) {
				for (int j = yNum - 1; j > i; j--) {
					if (0 == animalIndexArray [col, j]) {
						animalIndexArray [col, j] = animalIndexArray [col, i];
						animalIndexArray [col, i] = 0;
						animalObjArray [col, j] = animalObjArray [col, i];
						animalObjArray [col, i] = null;

						Vector2 endPos = getBlockWorldPos (col, j);
						if (animalObjArray [col, j] == null) {
							if (isDebug) Debug.Log ("oneColFallDown animalObjArray [" + col + ", " +  j + "] is null");
							return;
						}

						if (fallDownFinishCalled) {
							iTween.MoveTo (animalObjArray [col, j],
								iTween.Hash ("y", endPos.y, "x", endPos.x, "delay", .2));
						} else {
							fallDownFinishCalled = true;
							iTween.MoveTo (animalObjArray [col, j],
								iTween.Hash ("y", endPos.y, "x", endPos.x, "delay", .2,
									"oncomplete", "fallDownFinish", 
									"oncompleteparams", col,
									"oncompletetarget", gameObject));
						}
						break;
					}
				}
			}
		}
		if (!fallDownFinishCalled) {
			fallDownFinish (col);
		}
	}

	void fallDownFinish(int col){
		for (int i = yNum - 1; i >= 0; i--) {
			if (0 == animalIndexArray [col, i]) {
				// 生成新的动物并从顶部移动到col，i的位置
				createNewAndFallDown(col, i);
			}
		}

		updateDebugToUI ();
	}

	void createNewAndFallDown(int x, int y){
		int index = getRandomWhenClear ();
		if (isDebug) Debug.Log ("createNewAndFallDown at[" + x + ", " + y + "] animal=" + idleAnimals[index - 1].name);
		GameObject child = createGameObjAtPos (idleAnimals[index - 1], x, 0);
		animalObjArray [x, y] = child;
		animalIndexArray [x, y] = index;

		Vector2 pos = getBlockWorldPos (x, y);
		iTween.MoveTo (child,
			iTween.Hash ("y", pos.y, "x", pos.x, "delay", .2));
	}

	int randomRecord = -1;
	int getRandomWhenClear(){
		if (testMode) {
			randomRecord++;
			return randomRecord % 3 + 1;
		} else {
			return Random.Range (1, idleAnimals.Length);
		}
	}

	string getAnimalIndexArrayString(int[,] array) {
		string text = "";
		for (int i = 0; i < yNum; i++) {
			for (int j = 0; j < xNum; j++) {
				text += (array[j, i] == -1 ? "      " : (array[j, i]) + ",   ");
			}
			text += "\n";
		}
		return text;
	}

	void updateDebugToUI(){
		string text = getAnimalIndexArrayString (animalIndexArray);
		DebugUtils.textToUI (debugText, text);
	}
}