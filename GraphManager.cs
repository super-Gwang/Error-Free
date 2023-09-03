using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GraphManager : MonoBehaviour {
	public SenserData _senserData;
	public TMPro.TMP_Dropdown _Dropdown;

	public GameObject linerenderer;
	public GameObject pointer;

	public GameObject pointerRed;
	public GameObject pointerBlue;

	public GameObject HolderPrefb;

	public GameObject holder;
	public GameObject xLineNumber;

	public Material bluemat;
	public Material greenmat;

	public Text topValue;

	public List<GraphData> graphDataPlayer1 = new List<GraphData>();
	public List<GraphData> graphDataPlayer2 = new List<GraphData>();

	private GraphData gd;
	private float highestValue = 56;

	public Transform origin;

	private float lrWidth = 0.3f;
	private int dataGap = 0;
	public float gapValue;

	int index;

	void OnEnable()
	{
		// graphDataPlayer1.Clear();

		// for (int i = 0; i < 120; i++)
		// {
		// 	GraphData gd = new GraphData();
		// 	gd.marbles = Random.Range(10, 47);
		// 	graphDataPlayer1.Add(gd);
		// }
		// ShowGraph();
	}

	void OnDisable()
	{
		ClearGraph();
	}

	public void SetData(List<string> data_Tp, List<string> data_Hm)
	{
		graphDataPlayer1.Clear();
		graphDataPlayer2.Clear();

		for (int i = 0; i < data_Tp.Count; i++)
		{
			GraphData gd = new GraphData();
			if (data_Tp[i] == "-")
				continue;
			else
				gd.marbles = float.Parse(data_Tp[i]);

			graphDataPlayer1.Add(gd);
		}
		for (int i = 0; i < data_Hm.Count; i++)
		{
			GraphData gd = new GraphData();
			if (data_Hm[i] == "-")
				continue;
			else
				gd.marbles = float.Parse(data_Hm[i]);

			graphDataPlayer2.Add(gd);
		}
		ShowGraph();
	}	

	public void ShowGraph()
	{
		int count_1 = graphDataPlayer1.Count;
		int count_2 = graphDataPlayer1.Count;

		//holder가 있을 경우  제거
		ClearGraph();
		if(count_1 >= 1){
		// if(graphDataPlayer1.Count >= 1){

			//영점좌표를 찍어주는부분
			holder = Instantiate(HolderPrefb,Vector3.zero,Quaternion.identity) as GameObject;

			holder.name = "holder";
			holder.layer = 9;

			GraphData[] gd1 = new GraphData[count_1];
			GraphData[] gd2 = new GraphData[count_2];
			//초기화한배열 값을 넣어준다
			for(int i = 0; i < count_1; i++)
			{
				GraphData gd = new GraphData();
				gd.marbles = graphDataPlayer1[i].marbles;
				gd1[i] = gd;
			}
			for(int i = 0; i < count_2; i++)
			{
				GraphData gd = new GraphData();
				gd.marbles = graphDataPlayer2[i].marbles;
				gd2[i] = gd;
			}
			float gap_1 = count_1 * gapValue;
			float gap_2 = count_2 * gapValue;
			ShowData(gd1,gd2,gap_1,gap_2);
		}
	}

	public void ShowData(GraphData[] gdlist_Tp, GraphData[] gdlist_Hm, float gap_1, float gap_2) 
	{
		// Adjusting value to fit in graph
		for(int i = 0; i < gdlist_Tp.Length; i++)
		{
			// since Y axis is from 0 to 7 we are dividing the marbles with the highestValue
			// so that we get a value less than or equals to 1 and than we can multiply that
			// number with Y axis range to fit in graph. 
			// e.g. marbles = 90, highest = 90 so 90/90 = 1 and than 1*7 = 7 so for 90, Y = 7
			gdlist_Tp[i].marbles = (gdlist_Tp[i].marbles/highestValue)*7;
		}
		for(int i = 0; i < gdlist_Hm.Length; i++)
		{
			// since Y axis is from 0 to 7 we are dividing the marbles with the highestValue
			// so that we get a value less than or equals to 1 and than we can multiply that
			// number with Y axis range to fit in graph. 
			// e.g. marbles = 90, highest = 90 so 90/90 = 1 and than 1*7 = 7 so for 90, Y = 7
			gdlist_Hm[i].marbles = (gdlist_Hm[i].marbles/highestValue)*7;
		}
		StartCoroutine(BarGraphBlue(gdlist_Tp,gap_1));
		// StartCoroutine(BarGraphGreen(gdlist_Hm,gap_2));
	}

	public void ClearGraph(){
		if(holder)
			Destroy(holder);
	}

	IEnumerator BarGraphBlue(GraphData[] gd,float gap)
	{
		//갭만큼 일정위치로 이동
		float xIncrement = 0;
		int dataCount = 0;
		bool flag = false;
		
		//좌표의 시작점 1번값에 대한 위치
		Vector3 startpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));//origin.position;//

		while(dataCount < gd.Length)
		{
			Vector3 endpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));
			startpoint = new Vector3(startpoint.x,startpoint.y,origin.position.z);
			// pointer is an empty gameObject, i made a prefab of it and attach it in the inspector
			GameObject p = Instantiate(pointer, new Vector3(startpoint.x, startpoint.y, origin.position.z),Quaternion.identity) as GameObject;
			p.transform.parent = holder.transform;
			p.layer = 9;

			GameObject lineNumber = Instantiate(xLineNumber, new Vector3(origin.position.x+xIncrement, origin.position.y-0.18f , origin.position.z),Quaternion.identity) as GameObject;
			lineNumber.transform.parent = holder.transform;
			lineNumber.GetComponent<TextMesh>().text = (dataCount+1).ToString();
			lineNumber.layer = 9;

			// linerenderer is an empty gameObject with Line Renderer Component Attach to it, 
			// i made a prefab of it and attach it in the inspector
			GameObject lineObj = Instantiate(linerenderer,startpoint,Quaternion.identity) as GameObject;
			lineObj.transform.parent = holder.transform;
			lineObj.name = dataCount.ToString();
			lineObj.layer = 9;

			LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
			
			lineRenderer.material = bluemat;
			lineRenderer.SetWidth(lrWidth, lrWidth);
			lineRenderer.SetVertexCount(2);

			while(Vector3.Distance(p.transform.position,endpoint) > 0.2f)
			{
				float step = 5 * Time.deltaTime;
				p.transform.position = Vector3.MoveTowards(p.transform.position, endpoint, step);
				lineRenderer.SetPosition(0, startpoint);
				lineRenderer.SetPosition(1, p.transform.position);
				
				yield return null;
			}
			
			lineRenderer.SetPosition(0, startpoint);
			lineRenderer.SetPosition(1, endpoint);
			
			
			p.transform.position = endpoint;
			GameObject pointered = Instantiate(pointerRed,endpoint,pointerRed.transform.rotation) as GameObject ;
			pointered.transform.parent = holder.transform;
			startpoint = endpoint;

			dataCount++;
			xIncrement+= gapValue;
			yield return null;
		}
	}

	IEnumerator BarGraphGreen(GraphData[] gd, float gap)
	{
		float xIncrement = gap;
		int dataCount = 0;
		bool flag = false;

		Vector3 startpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));
		while(dataCount < gd.Length)
		{
			
			Vector3 endpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));
			startpoint = new Vector3(startpoint.x,startpoint.y,origin.position.z);
			// pointer is an empty gameObject, i made a prefab of it and attach it in the inspector
			GameObject p = Instantiate(pointer, new Vector3(startpoint.x, startpoint.y, origin.position.z),Quaternion.identity) as GameObject;
			p.transform.parent = holder.transform;
			
			// linerenderer is an empty gameObject with Line Renderer Component Attach to it, 
			// i made a prefab of it and attach it in the inspector
			GameObject lineObj = Instantiate(linerenderer,startpoint,Quaternion.identity) as GameObject;
			lineObj.transform.parent = holder.transform;
			lineObj.name = dataCount.ToString();
			
			LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
			
			lineRenderer.material = greenmat;
			lineRenderer.SetWidth(lrWidth, lrWidth);
			lineRenderer.SetVertexCount(2);

			while(Vector3.Distance(p.transform.position,endpoint) > 0.2f)
			{
				float step = 5 * Time.deltaTime;
				p.transform.position = Vector3.MoveTowards(p.transform.position, endpoint, step);
				lineRenderer.SetPosition(0, startpoint);
				lineRenderer.SetPosition(1, p.transform.position);
				
				yield return null;
			}
			
			lineRenderer.SetPosition(0, startpoint);
			lineRenderer.SetPosition(1, endpoint);
			
			
			p.transform.position = endpoint;
			GameObject pointerblue = Instantiate(pointerBlue,endpoint,pointerBlue.transform.rotation) as GameObject; 
			pointerblue.transform.parent = holder.transform;
			startpoint = endpoint;

			if(dataGap > 1){
				if((dataCount+dataGap) == gd.Length){
					dataCount+=dataGap-1;
					flag = true;
				}
				else if((dataCount+dataGap) > gd.Length && !flag){
					dataCount =	gd.Length-1;
					flag = true;
				}
				else{
					dataCount+=dataGap;
					if(dataCount == (gd.Length-1))
						flag = true;
				}
			}
			else
				dataCount+=dataGap;

			xIncrement+= gap;
			
			yield return null;
			
		}
	}

	public class GraphData
	{
		public float marbles;
	}
}
