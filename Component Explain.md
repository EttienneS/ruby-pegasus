# UI:

'n Unity canvas wat elke UI element bevat, die order panel onder, die mana en time panels, die mini-map en enige ander ding wat vir die user gewys word maar nie deel van die world is nie.

# FileController:

Hy is like die eerste ding wat load, hy gaan deur elke .json file in resources en load die info vir die creatures en structures.  So as jy in die structures 'n nuwe file maak en hom noem Kaas.json sal hy hom try load en hom add in die lys van avaialble structures.

# SpriteStore:

Baie selfde as file controller maar net spesifiek vir images, hy load al die terain tiles, structures en whatever else in die sprites folder is.  As iets hom vra vir 'n image wat hy nie kry nie maak hy so lime groen 'placeholder' image.

# Map:

Die is die actual map van die game, dit is wat mens noem 'n TileMap, basically 'n grid van images wat lanks mekaar gesit is in 'n 2D array, elke Map tile is binne die en in code kan ek dit change.

# SunController:

    Die gaan probably weg gaan eventually maar dit is die ding wat maak dat daar day/night cycles is.

# MiniMapCamera / Main MiniMapCamera

'n Camera is exactly wat dit klink, dis iets wat kyk na die wereld wat ek maak, as die game run en jy gaan na die scene view en jy sit hom op 3D kan jy actually alles sien hoe dit rerig lyk, dis 'n page wat rond float in space en die camera limit hoe die player dit sien.  Die MiniMapCamera is letterlik net 'n camera wat alles check van baie hoog af en dit in die hoekie display.

# CreatureController:

Spawn creatures en manage hulle state (as hulle destroy word, load, save).  Note: Elke creature is 'n autonomous enitity wat doen whatever hy wil based op sy Behavior (daar is currently net 'n Person en Wraith behaviour).

# GameController:

Die handle user input en hy coordinate die systems, 'n creature vra die game controller byvoorbeeld om hom te point na die structure controller as hy wil weet of iets 'n structure gebruik.

# Structure Controller:

Baie selfde as creature maar hy het ook twee TileMaps wat bo op die map layer le.  Vir performance reasons is elke structure nie 'n game object nie, daar is 1 ding wat almal control.  Structures het ook nie 'n will van hulle ie soos craetures nie alhoewel hulle baie ander characteristics share met creatures.

# SaveManager

Handle save/load, NOTE: dis currently fucked.  DIs baie effort om te doen so ek gana so elke nou en dan en sit dit terug en dan breek ek dit weer so ek los dit tot dit nodig is maar die base is in vir dit.

# MaterialController:

Unity gebruik materials om die display van 'n object te affect, daai swirly effect op die ley lines en daai wacky blokkie pers shroud shit is net materials bo op normal images.

# Magic Controller:

Die is om magical effects te manage, goed soos die bind runes en stuff wat structures kan doen, dis basically net ook vir performance, omdat meeste sturctures nie magic doen nie is dit dom om almal di eoption gee, so as een moet magic dan doen hy dit via dia.  Sien dit as soos alle 'environmental' magic.

# Effect Controller:

Baie keer kort mens net 'n explosion of like text op die screen, die ding beheer dit en maak dat hulle weer weg gaan na 'n sekere tyd.  Daai floaty runes as magic invoke word kom van die af.lanks

# LeyLineController:

Beheer alle line effects, die moet probably saam effect controller smelt met tyd maar ek hoop nog dat leyline tipe shit meer van 'n game effect gaan he.

# PhysicsController:

Baie soos die magic controller maar vir effects soos fire/water/temperature wat op een plek moet genamage word en performance hits kan veroorsaak.