using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* 此程式用來 尋找最短路徑(2D) */
/* 多載一個靜態function AStar()*/
/* 使用A Star演算法 但需初始化數值 */
/* 單次尋找最多只能生成maxNode(預設為120)個node 若超過會回傳null*/
/* 使用者可以藉由呼叫*/

public class AStarPathFind : MonoBehaviour {
    static int maxCol = 20;
	static int maxRow = 20;
    static int maxNode = 120;

    static List<AStartStruct> nodes = new List<AStartStruct>();
	static AStartStruct node;
    //A* 使用結構 
	class AStartStruct{
		public int currentX; //紀錄node位置
		public int currentY;
        public int alreadyWalkDis;//走過的距離
		public int estimateDis;//預測的距離
        public List<Vector2> walkedPosition;//走過的點
		public List<PathData.MoveDirection> walkRecord;//走過的方向
	}
    //傳入兩個二維座標 計算"預測最短距離"(D = |x2 - x1| + |y2 - y1|)
	private static int CaculateDis( int sourceX, int sourceY, int targetX, int targetY){
		int x = sourceX - targetX;
		int y = sourceY - targetY;
		if (x < 0)
			x *= -1;
		if (y < 0)
			y *= -1;
		return x + y;
	}

	//限制值的初始化 在GameManager OnSetMap時 有被呼叫
	public static void InitialInitailStaticVal(int col , int row ,int maxNode){
		AStarPathFind.maxCol = col;
		AStarPathFind.maxRow = row;
		AStarPathFind.maxNode = maxNode;
	}


    //判斷是否能產生一個Node 
    //此function 傳入一個 原始點node 跟要走的方向 與 障礙物地圖 並判斷是否能產生點 回傳 
    //ture代表可以 false代表不能
	private static bool CanGenerateNode(int x, int y,PathData.MoveDirection dir, AStartStruct node ,bool[,] mapObstacle)
    {
		
		int targetX = x + (int)PathData.PathRecognize (dir).x;//計算走一步後的
		int targetY = y + (int)PathData.PathRecognize (dir).y;
        Vector2 targetVector2 = new Vector2(targetX,targetY);//存成Vector2 


        if (node.walkedPosition != null)    // 若為true 代表此node 裡面可能有
            if (node.walkedPosition.Contains(targetVector2) == true) //若為true 代表想要產生的Node在以前走過的點上
                return false; 

		if (targetX < 0 || targetY < 0 || targetX >= maxCol || targetY >= maxRow) { //若為true 代表超越界線
			return false;
		} else if (mapObstacle [targetX, targetY] == true) {    //若為true 代表有障礙物
			return false;
		} else {    //若進入這邊 代表 此想產生的點沒走過, 沒過界, 沒在障礙物上
			return true;
		}
	}

    //此function 回傳 生成後的node
    private static AStartStruct GenerateNode(int x, int y, PathData.MoveDirection dir, int targetX, int targetY, AStartStruct node) {
        //生成node的位子
        int nextX = x + (int)PathData.PathRecognize(dir).x;
        int nextY = y + (int)PathData.PathRecognize(dir).y;
        //結構給予位置
        AStartStruct newNode = new AStartStruct();
        
        //結構給予值
        newNode.currentX = nextX;
        newNode.currentY = nextY;
        newNode.alreadyWalkDis = node.alreadyWalkDis + 1;
        newNode.estimateDis = CaculateDis(nextX, nextY, targetX, targetY);
        
        //檢查上個點有無資料 並複製過去 
        if (node.walkRecord != null)
        {
            newNode.walkRecord = new List<PathData.MoveDirection>(node.walkRecord.ToArray());
        }
        else
        {
            newNode.walkRecord = new List<PathData.MoveDirection>();
        }

        if (node.walkedPosition != null)
        {
            newNode.walkedPosition = new List<Vector2>(node.walkedPosition.ToArray());
        }
        else
        {
            newNode.walkedPosition = new List<Vector2>();
        }
        Vector2 nodePosition = new Vector2(node.currentX, node.currentY);

        //複製完後加入現在的值
        newNode.walkedPosition.Add(nodePosition);
		newNode.walkRecord.Add (dir);
        
		return newNode;
	}

    //選出 當前走過+預測直線距離 最小的node 
    //且此node為 月新的node越好 (理論上) 
	private static AStartStruct SelectBestNode(){
		AStartStruct bestNode = null;
		int bestN = 0;
		foreach (AStartStruct node in nodes) {
			if (bestNode == null) {
				bestNode = node;
				bestN = bestNode.alreadyWalkDis + bestNode.estimateDis;
			}
			else {
				int n = node.alreadyWalkDis + node.estimateDis;
				if (n <= bestN) {
					bestN = n;
					bestNode = node;
				}
			}
		}
		return bestNode;
	}


    /// <summary>AStar演算法2D 
    /// <para> sourcePostion :  起始點</para>
    /// <para> targetPosition : 目標點</para>
    /// <para> mapBlock : 障礙物地圖 </para>
    /// </summary>
	public static List<PathData.MoveDirection> AStar( Vector3 sourcePostion, Vector3 targetPosition,bool[,] mapBlock){
        System.GC.Collect();
        AStarPathFind.nodes.Clear ();

        int sourceX = (int)sourcePostion.x;
		int sourceY = (int)sourcePostion.y;
		int targetX = (int)targetPosition.x;
		int targetY = (int)targetPosition.y;
		bool[,] mapObstacle = new bool[maxCol, maxRow]; //default = false ; false means nothing Objstacle otherwise true;

        mapObstacle = mapBlock.Clone() as bool[,] ;

        //先將目標點改為非障礙物
		mapObstacle[targetX,targetY] = false;
		node = new AStartStruct ();
        //初始化第一個node
		node.alreadyWalkDis = 0;
		node.walkRecord = null;
		node.walkedPosition = null;
		node.estimateDis = CaculateDis (sourceX, sourceY, targetX, targetY);
        //開始進入 AStar 遞迴
		return AStarExecute( sourceX, sourceY, targetX, targetY, node , mapObstacle);

	}

    /// <summary>AStar演算法2D 
    /// <para> sourceX : 起始x</para>
    /// <para> sourceY : 起始y</para>
    /// <para> targetX : 目標x</para>
    /// <para> targetY : 目標y</para>
    /// <para> mapBlock : 障礙物地圖 </para>
    /// </summary>
	public static List<PathData.MoveDirection> AStar( int sourceX, int sourceY, int targetX, int targetY,bool[,] mapBlock){
        System.GC.Collect();
        AStarPathFind.nodes.Clear ();

        bool[,] mapObstacle = new bool[maxCol,maxRow]; //default = false ; false means nothing Objstacle otherwise true;
		mapObstacle = mapBlock.Clone() as bool[,] ;
        mapObstacle[targetX,targetY] = false;
        node = new AStartStruct ();
        
		node.alreadyWalkDis = 0;
		node.walkRecord = null;
        node.walkedPosition = null;
        node.estimateDis = CaculateDis (sourceX, sourceY, targetX, targetY);
		return AStarExecute( sourceX, sourceY, targetX, targetY, node , mapObstacle);
	}

    //此為真正的AStar遞迴 function
	private static List<PathData.MoveDirection> AStarExecute( int sourceX, int sourceY, int targetX, int targetY ,AStartStruct node, bool[,] mapObstacle){
		if (sourceX == targetX && sourceY == targetY) { // 若為ture 代表node位子到達target位子,此時已找到結果結束遞迴 回傳走過的路徑
			return node.walkRecord;
		}
		else {
            //下方4個if 傳入 4種方向 測試是否能創造node 並創造, 存在 nodes List裡面
			if (CanGenerateNode (sourceX, sourceY, PathData.MoveDirection.Up,node,mapObstacle)) {
				AStartStruct newNode = GenerateNode (sourceX, sourceY, PathData.MoveDirection.Up, targetX, targetY, node);
				nodes.Add (newNode);
            }
			if (CanGenerateNode (sourceX, sourceY, PathData.MoveDirection.Down, node,mapObstacle)) {
				AStartStruct newNode = GenerateNode (sourceX, sourceY, PathData.MoveDirection.Down, targetX, targetY, node);
				nodes.Add (newNode);
            }
			if (CanGenerateNode (sourceX, sourceY, PathData.MoveDirection.Left, node,mapObstacle)) {
				AStartStruct newNode = GenerateNode (sourceX, sourceY, PathData.MoveDirection.Left, targetX, targetY, node);
				nodes.Add (newNode);
            }
			if (CanGenerateNode (sourceX, sourceY, PathData.MoveDirection.Right, node,mapObstacle)) {
				AStartStruct newNode = GenerateNode (sourceX, sourceY, PathData.MoveDirection.Right, targetX, targetY, node);
				nodes.Add (newNode);
            }
            //在最佳node創造好4個node之後 選相對最好的node , 並將該於nodesList 移除
			node = SelectBestNode ();
			
            //如果 nodes裡面的量過多 就回傳null 表示 最近的目標可能離該物件太遠
            if (nodes.Count > maxNode)
                return null;
             
            nodes.TrimExcess ();

            //若在最後 創造4格node都失敗 nodes.Count 會=0 回傳null
            if (nodes.Count == 0)
                return null;

            nodes.Remove(node);
            return AStarExecute(node.currentX, node.currentY, targetX, targetY, node,mapObstacle);
		}
	}





}
