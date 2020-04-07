<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Untitled Document</title>
</head>

<body>
<?php
if (isset($_POST["button"]) && $_POST["button"]=="Submit")
{
	//var_dump($_POST);
	if ($_POST["DragCube"]>'')
	{
		echo '<h2>Calculations</h2> <br>';
		echo 'Original Drag Cube: '.$_POST["DragCube"].'<br>';
		$firstcut = explode('=',$_POST["DragCube"]);
		$secondcut = explode(',',$firstcut[1]);
		$title = $secondcut[0];
		$outstring = 'cube = '.$title;
		$looper = 1;
		for ($x=1;$x<19;$x++)
		{
			switch ($looper)
			{
				case 1:
				{
					$newvalue = $secondcut[$x] * (floatval($_POST["AreaModifier"] / 100));
					echo 'Area: '.$secondcut[$x].' -> '.$newvalue.'&nbsp;&nbsp;&nbsp;';
					break;
				}
				case 2:
				{
					$newvalue = $secondcut[$x] * (floatval($_POST["DragPercentage"] / 100));
					echo 'Drag: '.$secondcut[$x].' -> '.$newvalue.'&nbsp;&nbsp;&nbsp;';
					break;
				}
				case 3:
				{
					$newvalue = $secondcut[$x] * (floatval($_POST["DepthPercentage"] / 100));
					echo 'Depth: '.$secondcut[$x].' -> '.$newvalue.'<br>';
					break;
				}
			}
			$outstring .= ',';
			$looper++;
			if ($looper > 3)
			{
				$looper = 1;
			}
			$outstring .= $newvalue;
		}
		for ($x=19;$x<25;$x++)
		{
			$outstring .= ','.$secondcut[$x];
		}
		echo '<br>';
		echo '<h2>New Drag Cube</h2><br>';
		echo 'DRAG_CUBE<br>';
		echo '{<br>';
		echo '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'.$outstring.'<br>';
		echo '}<br>';
		echo '<hr>';
	}
}
if (isset($_POST["AreaModifier"]))
{
	$v1 = $_POST["AreaModifier"];
}
else
{
	$v1 = 100;
}
if (isset($_POST["DragPercentage"]))
{
	$v2 = $_POST["DragPercentage"];
}
else
{
	$v2 = 100;
}
if (isset($_POST["DepthPercentage"]))
{
	$v3 = $_POST["DepthPercentage"];
}
else
{
	$v3 = 100;
}
?>
<form id="form1" name="form1" method="post" action="">
<h2> Fengist's Drag Cube Modifier </h2>
This is a KSP Drag Cube modifier.  It's purpose is to simply recalculate drag cubes based on a percentage of the original values.  Copy the single line representing the drag cube from the PartDatabase.cfg and paste it in the text box below.  Change the percentages to the new desired value and click submit.
  <p>Paste your one line from your Drag Cube below.</p>
  <p>It should look like this:</p>
  <p>cube = Default, 14.605,0.3707,4.811, 14.605,0.3729,4.811, 18.040,0.31955,5.828, 18.040,0.28225,2.854, 14.71,0.3717,4.811, 14.71,0.3728,4.811, 0,3.73,0, 7.236,7.46,7.236
</p>
  <p>Original Drag Cube
    <input name="DragCube" type="text" id="DragCube" size="100" maxlength="200" />
  </p>
  Enter in the percentage change you'd like to make to each of the 3 values.
  <p>Area Percentage Modifier
    <input name="AreaModifier" type="text" id="AreaModifier" value="<?php echo $v1; ?>" maxlength="3" />
  </p>
  <p>Drag Percentage Modifier
    <input name="DragPercentage" type="text" id="DragPercentage" value="<?php echo $v2; ?>" maxlength="3" />
  </p>
  <p>Depth Percentage Modifier
<input name="DepthPercentage" type="text" id="DepthPercentage" value="<?php echo $v3; ?>" maxlength="3" />
  </p>
  <p>
    <input type="submit" name="button" id="button" value="Submit" />
  </p>
</form>
</body>
</html>