using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTower : MonoBehaviour
{
    [HeaderAttribute("Player Character")]
    public GameObject playerPrefab;
    private GameObject player;

    #region TowerGeneration
    public List<KeyValuePair<int, Transform>> playerOrderDestination = new List<KeyValuePair<int, Transform>>();

    [HeaderAttribute("Tower Module Listing")]
    public GameObject[] cornerModules;
    public GameObject[] cornerSteps;

    public GameObject[] faceModules;

    public GameObject[] puzzleIn;
    public GameObject[] puzzleOut;

    public GameObject[] roofModules;

    //Reference to puzzle after tower is generated
    public List<GameObject> puzzleGenInput = new List<GameObject>();
    public List<GameObject> puzzleGenOutput = new List<GameObject>();

    //Checj for puzzle generation
    public List<bool> isInputGen = new List<bool>();
    public List<bool> isOutputGen = new List<bool>();

    //List of all modules, key = layer, value = module transform
    public List<KeyValuePair<int, Transform>> moduleList = new List<KeyValuePair<int, Transform>>();

    //Static bool to check if Tower is finished generating
    static public bool isConstructed;

    [HeaderAttribute("Tower Generation Parameters")]
    public int towerPuzzleLayers;
    public int towerDecorativeLayers;
    public int modulesPerLayer;
    public int buildingSpeed;
    public int characterMovementSpeed;

    //Start of the game and Movement of the Player

    //Most of the code to be added is here in regards to game completion etc...
    public IEnumerator MovePlayerToWayPoint(KeyValuePair<int, Transform>[] waypoints, float rotationSpeed, float movementSpeed)
    {
        for (int i = 0; i < waypoints.Length; ++i)
        {
            //ignore
            print(waypoints[i].Value.CompareTag("Output"));

            //Check for current layer to render steps
            foreach (var module in moduleList)
            {
                MeshRenderer[] renderers = module.Value.GetComponentsInChildren<MeshRenderer>(true);

                if (module.Key == waypoints[i].Key)
                {
                    foreach (var renderer in renderers)
                    {
                        if (renderer.gameObject.CompareTag("Steps") && renderer.enabled == false)
                        {
                            renderer.enabled = true;

                            yield return BlockSpawn(renderer.gameObject, buildingSpeed);
                        }
                    }
                }

                //Goofy aah despawn code that didnt work properly just ignore
                //else
                //{
                //    foreach (var renderer in renderers)
                //    {
                //        if (renderer.gameObject.CompareTag("Steps") && renderer.enabled == true)
                //        {
                //            yield return BlockSpawn(renderer.gameObject, 20, false);

                //            renderer.enabled = false;
                //        }
                //    }
                //}
            }

            //rotation once at a waypoint
            Vector3.RotateTowards(player.transform.forward, waypoints[i].Value.forward, Time.deltaTime * rotationSpeed, 0.0f);

            //Player movement code
            while (Vector3.Distance(player.transform.position, waypoints[i].Value.transform.position) > 0.1)
            {
                player.transform.position = Vector3.Lerp(player.transform.position, waypoints[i].Value.position, movementSpeed * Time.deltaTime);

                yield return null;
            }

            //Checks for an output waypoint, if at an output checks for puzzle completion
            if (waypoints[i].Value.CompareTag("Output"))
            {
                print("entered");

                //Initialising baseclass input
                BaseInput input = null;


                //Check for input linked with given output
                foreach (GameObject puzzle in puzzleGenInput)
                {
                    BaseInput inputTemp = puzzle.GetComponent<BaseInput>();

                    print(inputTemp.output);
                    print(waypoints[i].Value.parent);

                    if (inputTemp.output.transform == waypoints[i].Value.parent)
                    {
                        input = inputTemp;
                    }
                }

                //Checks for input completion, will loop until puzzle completed
                while (!input.isCompleted)
                {
                    print(input);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        ///////////////////
        //Game Completion//
        ///////////////////
        
        //TODO code here for feedback etc...

        print("completed game");
    }

    //Dont touch, needed for generation due to coroutine shenanigans
    private Vector3 nextPosition;

    //Tower Construction
    public IEnumerator ConstructTower(Vector3 generatePosition, int layers, int modulesPerLayer = 4, int decoLayers = 2)
    {
        //Initialise layer count and next position
        nextPosition = generatePosition;
        int totalLayerCount = 0;

        //Creates puzzle references
        for(int i = 0; i < layers; i++)
        {
            int moduleID = Random.Range(0, puzzleIn.Length);

            puzzleGenInput.Add(puzzleIn[moduleID]);
            puzzleGenOutput.Add(puzzleOut[moduleID]);

            isInputGen.Add(false);
            isOutputGen.Add(false);
        }

        //Puzzle Selection per layer
        for (int i = 0; i < layers; ++i)
        {
            int inputID = Random.Range(0, layers);
            int outputID = Random.Range(0, layers);

            //Checks for any generated puzzle templates - input
            while (isInputGen[inputID])
            {
                if (inputID >= layers - 1)
                {
                    inputID = 0;
                }
                else
                {
                    inputID++;
                }
            }

            //Checks for any generated puzzle templates - output
            while (isOutputGen[outputID])
            {
                if (outputID >= layers - 1)
                {
                    outputID = 0;
                }
                else
                {
                    outputID++;
                }
            }

            //Decorative layer Generation
            for (int j = 0; j < decoLayers; ++j)
            {
                yield return GenerateLayer(nextPosition, totalLayerCount, true, modulesPerLayer);

                totalLayerCount++;
            }

            //Check for roof generation
            if(i == layers-1)
            {
                yield return GenerateLayer(nextPosition, totalLayerCount, false, modulesPerLayer, i, i, true);
            } else
            {
                yield return GenerateLayer(nextPosition, totalLayerCount, false, modulesPerLayer, i, i);
            }

            totalLayerCount++;
        }

        //Finished Construction of tower
        isConstructed = true;

        //Game Start - Movement of the avatar begins
        yield return MovePlayerToWayPoint(playerOrderDestination.ToArray(), characterMovementSpeed, characterMovementSpeed);
    }

    //Creates a layer based on the start position of the first corner and the size of each face in module cubes
    //isDeco designates if the layer is only for decorative purposes => creates gaps between interactive layers
    public IEnumerator GenerateLayer(Vector3 startPosition, int layerID, bool isDeco, int size = 4, int inputID = 0, int outputID = 0, bool isRoof = false)
    {
        //////////////////////////
        //Initialising Variables//
        //////////////////////////
        
        //Stores last generated module for next module reference
        GameObject lastGeneratedModule = null;
        Vector3 nextPos = startPosition;

        //Stores corners - specific for roof generation
        Vector3 firstCorner = startPosition;
        Vector3 lastCorner = startPosition;

        //Gives position for starting the next layer's generation
        Vector3 nextLayerPos = Vector3.zero;

        //Checks if input or output have already been generated
        bool hasInput = false;
        bool hasOutput = false;

        //Determines random input face and output face
        int randomInputFace = Random.Range(0, 4);

        int randomOutputFace = Random.Range(0, 4);

        //Insurance for output/input not to be on the same side
        if (randomInputFace == randomOutputFace)
        {
            if (randomInputFace != 3)
                randomOutputFace++;
            else
                randomOutputFace = 0;
        }

        for (int j = 0; j < 4; ++j)
        {
            //Rotation set each time a side is complete
            float dRotation = (-90 * j);

            //Gets random corner module
            GameObject corner = cornerModules[Random.Range(0, cornerModules.Length)];

            //Stairs always generated at the start - j==0 -> Check for starting position
            if (j == 0)
                corner = cornerSteps[Random.Range(0, cornerSteps.Length)];


            //Initialises module rotation
            Quaternion cornerRot = Quaternion.Euler(corner.transform.rotation.x, corner.transform.rotation.y + dRotation, corner.transform.rotation.z);

            //Module Creation
            lastGeneratedModule = Instantiate(corner, nextPos, cornerRot, transform);
            yield return BlockSpawn(lastGeneratedModule, buildingSpeed);

            //Adds module to a list of objects based on layer
            moduleList.Add(new KeyValuePair<int, Transform>(layerID, lastGeneratedModule.transform));

            //Gets the position for next object
            nextPos = lastGeneratedModule.transform.GetChild(2).position;

            //Gives start position for the next layer, GetChild(0) being an empty game object referencing the top of the module
            if (j == 0)
            {
                nextLayerPos = lastGeneratedModule.transform.GetChild(0).position;

                if(layerID == 0)
                {
                    player = Instantiate(playerPrefab, lastGeneratedModule.transform.GetChild(3).position, playerPrefab.transform.rotation);
                }

                firstCorner = lastGeneratedModule.transform.position;
            }

            //For roof spawn calculations
            if (j == 2)
            {
                lastCorner = lastGeneratedModule.transform.position;
            }

            //Adds corner waypoinrs
            playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID, lastGeneratedModule.transform.GetChild(3)));

            int randomFaceSelection = Random.Range(0, size - 1);
            print(randomFaceSelection);

            ///////////////////
            //Face generation//
            ///////////////////
            for (int i = 0; i < size-1; ++i)
            {
                GameObject face = faceModules[Random.Range(0, faceModules.Length)];

                //Sets Face type if not decorative module
                if (!isDeco)
                {
                    if(!hasInput && j == randomInputFace && i == randomFaceSelection)
                    {
                        face = puzzleGenInput[inputID];
                    }

                    if (!hasOutput && j == randomOutputFace && i == randomFaceSelection)
                    {
                        face = puzzleGenOutput[outputID];
                    }
                }

                lastGeneratedModule = Instantiate(face, nextPos, cornerRot, transform);

                //Spawns Block
                yield return BlockSpawn(lastGeneratedModule, buildingSpeed);

                moduleList.Add(new KeyValuePair<int, Transform>(layerID, lastGeneratedModule.transform));

                //Checks if layer is decorative or not
                if (!isDeco)
                {
                    //Sets Generated Inputs as references rather than prefabs to check for puzzle completion later on in the game
                    if (!hasInput && j == randomInputFace && i == randomFaceSelection)
                    {
                        puzzleGenInput[inputID] = lastGeneratedModule;
                        isInputGen[inputID] = true;

                        if (puzzleGenInput[inputID].name == "BreakInputModule(Clone)")
                        {
                            playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID, lastGeneratedModule.transform.GetChild(3)));
                        }

                        if (isOutputGen[inputID])
                        {
                            if (puzzleGenInput[inputID].name == "BreakInputModule(Clone)")
                            {
                                lastGeneratedModule.GetComponent<BaseInput>().output = lastGeneratedModule;
                                lastGeneratedModule.GetComponent<BaseInput>().outputInstanceID = lastGeneratedModule.transform.GetInstanceID();

                                playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID, lastGeneratedModule.transform.GetChild(3)));
                            }
                            else
                            {
                                lastGeneratedModule.GetComponent<BaseInput>().output = puzzleGenOutput[inputID];
                                lastGeneratedModule.GetComponent<BaseInput>().outputInstanceID = puzzleGenOutput[inputID].transform.GetInstanceID();
                            }
                        }

                        hasInput = true;
                    }

                    //Sets Generated Outputs as references rather than prefabs to check for puzzle completion later on in the game
                    if (!hasOutput && j == randomOutputFace && i == randomFaceSelection)
                    {
                        puzzleGenOutput[outputID] = lastGeneratedModule;
                        isOutputGen[outputID] = true;

                        if(puzzleGenInput[outputID].name != "BreakInputModule(Clone)" || puzzleGenInput[outputID].name != "BreakInputModule")
                            playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID, lastGeneratedModule.transform.GetChild(3)));

                        if (isInputGen[outputID])
                        {
                            if(puzzleGenInput[inputID].name != "BreakInputModule(Clone)")
                            {
                                puzzleGenInput[outputID].GetComponent<BaseInput>().output = lastGeneratedModule;
                                puzzleGenInput[outputID].GetComponent<BaseInput>().outputInstanceID = lastGeneratedModule.transform.GetInstanceID();
                            }
                        }

                        hasOutput = true;
                    }
                }

                nextPos = lastGeneratedModule.transform.GetChild(2).position;

                if (j==3 && i == size - 2 && !puzzleGenOutput.Contains(lastGeneratedModule))
                {
                    playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID,lastGeneratedModule.transform.GetChild(3)));
                }
            }
        }

        //Roof Generation
        if (isRoof)
        {
            Vector3 centreLayerPosition = Vector3.Lerp(firstCorner, lastCorner, 0.5f);

            GameObject roofPrefab = roofModules[Random.Range(0, roofModules.Length)];

            GameObject roof = Instantiate(roofPrefab, centreLayerPosition, roofPrefab.transform.rotation, transform);

            yield return BlockSpawn(roof, buildingSpeed);

            roof.transform.localScale *= (size + 1);

            roof.transform.position = new Vector3(roof.transform.position.x, roof.transform.position.y + roof.transform.lossyScale.y/2 + lastGeneratedModule.transform.lossyScale.y/2, roof.transform.position.z);

            playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID, roof.transform.GetChild(3)));
            playerOrderDestination.Add(new KeyValuePair<int, Transform>(layerID, roof.transform.GetChild(4)));
        }

        yield return nextPosition = nextLayerPos;
    }

    //Spawns / Despawns Modules
    private IEnumerator BlockSpawn(GameObject module, float speed = 100, bool isSpawning = true)
    {
        Vector3 target = module.transform.localScale;
        Vector3 start = Vector3.zero;

        float lerpValue = 0;

        if (!isSpawning)
        {
            target = Vector3.zero;
            start = module.transform.localScale;
        }

        module.transform.localScale = start;

        while (lerpValue < 1)
        {
            lerpValue += speed / 100;
            module.transform.localScale = Vector3.Lerp(start, target, lerpValue);

            yield return null;
        }

        print("finished spawning");
    }

    private void Awake()
    {
        isConstructed = false;
    }

    //Creates Tower at the centre of the Prefab
    private void Start()
    {
        StartCoroutine(ConstructTower(Vector3.zero, towerPuzzleLayers, modulesPerLayer, towerDecorativeLayers));
    }
    #endregion
}
