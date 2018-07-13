using UnityEngine;
using System.Collections;

public struct Vector2Int{
	public int x;
	public int y;
	public Vector2Int(int xx, int yy){
		x = xx;
		y = yy;
	}
	public static Vector2Int operator +(Vector2Int v1, Vector2Int v2){
		return new Vector2Int (v1.x + v2.x, v1.y + v2.y);
	}
	public static Vector2Int operator -(Vector2Int v1, Vector2Int v2){
		return new Vector2Int (v1.x - v2.x, v1.y - v2.y);
	}
}
public struct ColorArray{
	public Color32[,] colors;

	public ColorArray(int size){
		colors = new Color32[size, size];
	}
}
public class Playercontroller : MonoBehaviour 
{	//random variables
	public float speed;
	public GUIText countText;
	public GUIText winText;
	private int count;
	public Material groundMat;
	public Texture2D txt;

	private Color32[] txtColors;
	private Color32[] tempColors;
	public Texture2D brush;
	private Color32[] brushColors;
	private int brushSize;
	private Transform tf;
	private Rigidbody rb;
	//the jump timer variables
	private float canJump = 0;
	private float jumpTime = 0;
	private float jumpTimeCap = 100;

	private bool applyNeeded = false;
	public Color paintCol;
	public Gradient gradCol;
	public float evalRate;
	private float evalcounter = 0;
	public int resolution = 512;
	private float resolutionfactor;
	private float resolutionSquared;
	public int halfRes;
	public int radius = 1;
	private int effectiveRadius;
	public float factor = 1f;

	private ColorArray[] brushMips;
	private int brushMaxSize = 60;

	void Start ()
	{	tf = GetComponent<Transform> ();
		rb = GetComponent<Rigidbody> ();
		count = 0;
		SetCountText ();
		winText.text = "";
		if (txt == null) {
			txt = new Texture2D (resolution, resolution);

		} else {
			Color32[] pix = txt.GetPixels32 ();
			txt = new Texture2D (txt.width, txt.width);
			txt.SetPixels32 (pix);
			resolution = txt.width;
			/*int count = txt.width;
			for (int i = 0; i < count; i++) {
				for (int j = 0; j < count; j++) {
					
				}
			}*/

		}
		brushColors = brush.GetPixels32 ();
		txtColors = txt.GetPixels32 ();
		int len = txtColors.Length;
		Color32 black = new Color32 (0, 0, 0, 255);
		for (int i = 0; i < len; i++) {
			txtColors [i] = black;

		}
		txt.SetPixels32 (txtColors);

		txt.Apply ();
		brushSize = brush.width;

		halfRes = resolution / 2;
		resolutionfactor = resolution / 20f;
		resolutionSquared = resolution * resolution;
		//groundMat.mainTexture = txt;
		groundMat.SetTexture ("_EmissionMap", txt);
		groundMat.SetColor ("_EmissionColor", Color.white);
		BrushMipArraySetup ();
	}

	void BrushMipArraySetup(){
		int brushMaxDiameter = brushMaxSize * 2 + 1;
		brushMips = new ColorArray[brushMaxDiameter + 1];
		for (int m = 0; m < brushMaxDiameter + 1; m++) {
			brushMips [m] = new ColorArray (m);
			float mRecip = 1f / (float)m;
			for (int i = 0; i < m; i++) {
				for (int j = 0; j < m; j++) {
					brushMips[m].colors[i,j] = brush.GetPixelBilinear ((float)i * mRecip, (float)j * mRecip);
				}
			}
		}
	}
	void Update(){
		
		if (applyNeeded) {
			txt.SetPixels32 (txtColors);
			txt.Apply ();
			applyNeeded = false;
			//txtColors = txt.GetPixels ();

		}
		evalcounter += evalRate;
		if (evalcounter > 1f) {
			evalcounter = 0;
		}
        
		paintCol = gradCol.Evaluate (evalcounter);
		effectiveRadius = Mathf.RoundToInt (rb.velocity.magnitude * radius * 0.4f + radius);
		if (effectiveRadius > brushMaxSize) {
			effectiveRadius = brushMaxSize;
		} 
	}
	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		//movement controllers
		rb.AddForce (movement * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(0.0f, 15f, 0.0f);
        }



	}
	void OnTriggerEnter(Collider other) 
	{
		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive (false);
			count = count + 3;
			SetCountText ();
		}
	}
	void OnCollisionStay(Collision other){
		if (other.gameObject.tag == "ground") {

			SetPixelCurrentPos ();
		}
		//if (Input.GetKey (KeyCode.Space)) {
		//	rb.AddForce(0.0f, 300f, 0.0f);
		//}
	}
	void SetPixelCurrentPos(){
		Vector2Int currentPos = new Vector2Int (Mathf.RoundToInt(tf.position.x * resolutionfactor + halfRes), Mathf.RoundToInt(tf.position.z * resolutionfactor + halfRes));
		int count = effectiveRadius * 2 + 1;
		int middle = effectiveRadius + 1;
		float radsquared = effectiveRadius * effectiveRadius;
		for(int i = 0; i < count; i++){
			for (int j = 0; j < count; j++) {
				
				int index = j * count + i;
				Color bil = brushMips[count].colors[i,j];

				int txtColorsIndex = (currentPos.y - effectiveRadius + j) * resolution + currentPos.x - effectiveRadius + i;
				if(txtColorsIndex < resolutionSquared && txtColorsIndex > -1){
					Color dest = txtColors[txtColorsIndex];
					Color c0 = bil.r *  factor * (paintCol - dest) + dest;
					txtColors [(currentPos.y + j - effectiveRadius) * resolution + (currentPos.x + i - effectiveRadius)] = c0;

				}
			}
		}
		applyNeeded = true;
	}

	void SetCountText ()
	{
		countText.text = "Count: " + count.ToString();
		if (count >= 20) {
			winText.text = "YOU WIN!";
			gameObject.SetActive (false);
		}
	}
}




/*pixels [i, j] = new Vector2Int (currentPos.x - effectiveRadius + i, currentPos.y - effectiveRadius + j);
				float x = currentPos.x - pixels [i, j].x;
				float y = currentPos.y - pixels [i, j].y;

				if( x * x + y * y < radsquared){
					txt.SetPixel (pixels [i, j].x, pixels [i, j].y, paintCol);

					applyNeeded = true;
				}*/