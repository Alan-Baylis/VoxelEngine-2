using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;
using System.Threading;
public class World : MonoBehaviour{
	private static World _instance;
	public GameObject chunkPrefab;
	public Chunk[,,] chunks = new Chunks[13, 3, 13];
    public static World instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<World>();
            return _instance;
        }
    }

    void Start(){
    	// for(int i = 0; i < 13; i++){
    	// 	for(int j = 0; j < 13; j++){
    	// 		CreateChunk(new Vector3(i,0,j));
		   //  	CreateChunk(new Vector3(i,1,j));
		   //  	CreateChunk(new Vector3(i,2,j)); 
		   //  	CreateChunk(new Vector3(i,-1,j));		
    	// 	}
    	// }
    	
    	// MakeTestChunkAO();	    	

    }

    void Update(){
    }
    public void DestroyChunk(Chunk chunk){
    	// chunks.Remove(chunk.pos);
    	chunk.delete = true;
    }
    private void MakeTestChunkAO(){
    	GameObject newChunk = Instantiate(chunkPrefab, new Vector3(0 , 0, 0), Quaternion.Euler(Vector3.zero)) as GameObject;
    	Chunk chunk = newChunk.GetComponent<Chunk>();
    	chunk.pos = new Vector3(0 , 0, 0);
    	for(int i = 0; i < 4; i++){
    		for(int j = 0; j < 4; j++){
    			chunk.blocks[i, 0, j] = new Block(1);
    		}
    	}

    	chunk.blocks[1, 0, 1] = new Block(1);
    	chunk.blocks[1, 1, 2] = new Block(1);
    	chunk.blocks[1, 1, 1] = new Block(1);
    	chunk.blocks[2, 1, 2] = new Block(1);
    	chunk.blocks[3, 1, 2] = new Block(1);
    	chunk.blocks[4, 1, 2] = new Block(1);
    	chunk.blocks[1, 1, 0] = new Block(1);
    	chunk.RenderChunk();
    }
    int stoneBaseHeight = -20;
    float stoneBaseNoise = 0.01f;
    int stoneBaseNoiseHeight = 5;

    int stoneMountainHeight = 10;
    float stoneMountainFrequency = 0.008f;
    int stoneMinHeight = 0;

    int dirtBaseHeight = 1;
    float dirtNoise = 0.02f;
    int dirtNoiseHeight = 1;
    public static int GetNoise(int x, int y, int z, float scale, int max, float power)
    {
        float noise = (Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f);

        noise = Mathf.Pow(noise, power);

        return Mathf.FloorToInt(noise);
    }

    int LayerStoneBase(int x, int z)
    {
        int stoneHeight = stoneBaseHeight;
        stoneHeight += GetNoise(x, 0, z, stoneMountainFrequency, stoneMountainHeight, 1.6f);
        stoneHeight += GetNoise(x, 1000, z, 0.03f, 8, 1) * 2;

        if (stoneHeight < stoneMinHeight)
            return stoneMinHeight;

        return stoneHeight; 
    }

    int LayerStoneNoise(int x, int z)
    {
        return GetNoise(x, 0, z, stoneBaseNoise, stoneBaseNoiseHeight, 1);
    }

    int LayerDirt(int x, int z)
    {
        int dirtHeight = dirtBaseHeight;
        dirtHeight += GetNoise(x, 100, z, dirtNoise, dirtNoiseHeight, 1);
       
        return dirtHeight;
    }
    public void CreateChunk(Vector3 pos){
    	GameObject newChunk = Instantiate(chunkPrefab, new Vector3(pos.x * 32f, pos.y * 32f, pos.z * 32f), Quaternion.Euler(Vector3.zero)) as GameObject;
    	Chunk chunk = newChunk.GetComponent<Chunk>();
    	chunk.pos = pos;
    	chunk.loaded = true;
    	// chunks.Add(pos, chunk);
    	Thread thread = new Thread(()=>{
    			GenerateChunk(pos, chunk);	
    	});
    	thread.Start();	
    }

    private void GenerateChunk(Vector3 pos, Chunk chunk){
		for(int x = 0; x < Chunk.Size; x++){
			for(int z = 0; z < Chunk.Size; z++){
				int stoneHeight = LayerStoneBase((int)(pos.x*32f) + x, (int)(pos.z*32f) + z);
		        stoneHeight += LayerStoneNoise((int)(pos.x*32f) + x, (int)(pos.z*32f) + z);

		        int dirtHeight = stoneHeight + LayerDirt((int)(pos.x*32f) + x, (int)(pos.z*32f) + z);
				for(int y = 0; y < Chunk.Size; y++){
					int i;
					if(y + (pos.y*32f) < dirtHeight){
						chunk.blocks[x, y, z] = new Block(1);
						if(y + (pos.y*32f) < stoneHeight){
							chunk.blocks[x, y, z] = new Block(2);
						}
					}else{
						chunk.blocks[x, y, z] = new Block(0);
					}
				}
			}
		}

		chunk.RenderChunk();
    }
}