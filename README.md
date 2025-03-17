I just thought to share this because why not. It's designed to be embedded into the Unity project.

Change the namespace as you wish.

This will yield an error due to an absence of files "Fujin.System.SceneLoadManager" and "Fujin.Constants.GameObjectTag", so replace them with something in your project.

The mechanism behind this implementation:
1) Control the availability of cursor by switching its lock state between locked and confined
2) Remain the visibility of cursor hidden and let an external sprite follow the cursor position to alleviate crappy move
3) Employ a different object (referred to as target in the code) when it's imperative to change the cursor speed / sensitivity

This is open for a pull request. I have to make a lot of modifications later on as I didn't implement a function to switch sprites based on the enumerator value (as I didn't have to); but as more features stack up, surely we need more methods built onto this.
