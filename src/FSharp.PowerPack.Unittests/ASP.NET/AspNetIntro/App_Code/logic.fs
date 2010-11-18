#light
module FSharpWeb.Logic

// Calculates a factorial
let rec factorial x = 
  if (x <= 1) then 1 else x * (factorial (x - 1))
  
// Loading data for the databinding sample  
type Colors = { Name:string; Rgb:string; }
let loadData () =  
  [ { Name="Green"; Rgb="#00ff00" };
    { Name="Red";   Rgb="#ff0000" };
    { Name="Blue";  Rgb="#0000ff" }; ]
    
  