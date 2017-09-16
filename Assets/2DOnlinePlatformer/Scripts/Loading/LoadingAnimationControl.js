#pragma strict
var frames : Texture[];
var framesPerSecond = 10;
 
function Update() {
    var index : int = (Time.time * framesPerSecond) % frames.Length;
    GetComponent.<Renderer>().material.mainTexture = frames[index];
}