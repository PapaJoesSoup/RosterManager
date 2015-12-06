RosterManager  
Kerbal Space Program Addon.  Manages all kerbals in a given save game.  Adds gameplay enhancements

License:  CC-BY-NC-SA 4.0

============
Version 0.2.0.0 06 Dec 2015 
 - Compatible with new KSP 1.0.5 Kerbal Traits/naming process.
 - Up to date with DeepFreeze Mod integration.
 - Fixed Filter tab at top of GUI, and added Frozen filter (if DeepFreeze is installed)
 - Added Salaries option to Settings, also in settings you can set salaries to be paid monthly or yearly.
   - Salaries are set via Training - Experience Tab in the GUI. Default value is 10000 funds.
   - Salaries are deducted monthly or yearly, but if you do not have enough funds then the kerbals who aren't paid will go into a contract dispute mode.
     - Contract Disputes occur when kerbals are not paid their salary. Each pay period the kerbals who aren't paid will ask for a payrise (which goes up each time they aren't paid).
       All un-paid salary will accrue each time they are not paid.
       The user can accept the payrise and the kerbal will continue to work for you. However, this will only occur for 3 pay periods. At the end of the third occurrence of them not
	   being paid the kerbal will go on strike (Become a tourist). Until such time as they are paid all owed money.
	 - A Contract dispute Window is available to see all kerbals that currently are in contract dispute and allow you to accept payrise, decline (they go on strike). Or if you have
	   funds available you can re-pay them all owed monies and they will go back to work for you.
	 - Each month or year (salary period) funds are checked and if there are enough contract disputes are automatically resolved. 
 - Significant refactoring of the code in prep for next development phase.
 - Added KerbalLifeSpanInfo including Age and lifespan processing setting for each kerbal. 
   - If this setting is on all kerbals in the game will age over time and eventually die from old age.
   - If the Repawn kerbal option is on in the main game settings then they will respawn when the die (but as a new freshman again).
   - If you also have DeepFreeze Mod intalled then freezing a kerbal will stop the aging process whilst they are frozen.
   - Each Kerbals age is shown in the Attributes Tab for each kerbal and the main roster list. But the age cannot be changed. 
   
Version 0.1.0.1 23 Jun 2015 - KSP 1.0.3 and DeepFreeze compatability
 - Updated DFInterface.dll to latest version
 - updated KSP assembly references to KSP ver 1.0.3

Version 0.1.0.0 20 Jun 2015 - Initial alpha release
 - Roster interface based on Roster window from Ship Manifest, but greatly enhanced (sorting, more detail)
 - In flight controls removed. These will stay in Ship Manifest.  Roster Manager will not be visible in Flight scene.
 - Kerbal manager Tab buttons for various areas to manage.
 - Support for DeepFreeze cryo canisters.  Frozen kerbals are displayed with assigned vessel. 
 - Attributes Tab, contains kerbal editing found in SM.  However, all kerbals can be edited.
 - Training Tab contains ability to edit kerbal skill and experience
 - History tab contains kerbal's flight history.



