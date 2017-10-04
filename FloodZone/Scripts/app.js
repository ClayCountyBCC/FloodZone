function Search(ev)
{
  ev.preventDefault();
  var e = document.getElementById('Error');
  e.innerText = "";
  e.style.display = "none";

  if (!Validate())
  {
    e.innerText = "Please check the values entered and try again.  The house number must be a number, and the street must not contain numbers."
    e.style.display = "block";
    return;
  }
  document.getElementById('Searching').style.display = "inline-block";
  var SearchAddress = {
    house: document.getElementById('house').value,
    street: document.getElementById('street').value.toUpperCase()
  }

  $.post(
    './API/Flood/',
    SearchAddress,
    Success,
    'json');
  return true;
}

function Success(data, status, jqxhr)
{
  BuildResults(data);
  document.getElementById('Searching').style.display = "none";
  console.log(data);
  if (data.length == 1)
  {
    Zoom(data[0].ToLatLong, data[0].FloodZone);
  }
}

function Validate()
{
  var housenumber = document.getElementById('house').value;
  var street = document.getElementById('street').value;
  return (housenumber.length > 0 && street.length > 0)
}

function BuildResults(data)
{
  var target = document.getElementById('results');
  target.innerHTML = "";
  var table = document.createElement("table");
  table.classList.add("table", "table-striped");
  table.appendChild(BuildTableHeaderRow());
  var tbody = document.createElement("tbody");
  if (data.length == 0)
  {
    var row = document.createElement("tr");
    var cell = document.createElement("td");
    cell.colSpan = 4;
    cell.appendChild(document.createTextNode("No data was found for this search."));
    row.appendChild(cell);
    tbody.appendChild(row);
  }
  else
  {
    data.map(function (d)
    {
      console.log('flood zone in data.map', d.FloodZone);
      var row = BuildTableRow(
        d.FloodZone,
        d.WholeAddress,
        d.City,
        d.Zip,
        d.ToLatLong,
        false);
      tbody.appendChild(row);
    });
  }
  table.appendChild(tbody);
  target.appendChild(table);
  target.style.display = "block";
}

function BuildTableHeaderRow()
{
  let thead = document.createElement("thead");
  let row = BuildTableRow("Flood Zone", "Whole Address", "City",
    "Zip", "", "", true);
  thead.appendChild(row);
  return thead;
}

function BuildTableRow(
  zone,
  address,
  city,
  zip,
  latlong,
  isHeader)
{
  var row = document.createElement("tr");
  var colTag = (isHeader ? "TH" : "TD");
  row.appendChild(createTableElement(zone, "15%", colTag));
  row.appendChild(createTableElement(address, "40%", colTag));
  row.appendChild(createTableElement(city, "20%", colTag));
  row.appendChild(createTableElement(zip, "10%", colTag));
  row.appendChild(createTableButton(latlong, zone, "15%", colTag));
  return row;
}

function createTableElement(value, width, colTag)
{
  var d = document.createElement(colTag);
  d.style.width = width;
  d.appendChild(document.createTextNode(value));
  return d;
}
function createTableButton(value, floodZone, width, colTag)
{
  console.log('createtablebutton flood zone', floodZone);
  if (colTag == "TH") return createTableElement("", width, colTag);
  var d = document.createElement(colTag);
  d.style.width = width;

  let add = document.createElement("button");
  add.type = "button";
  add.className = "btn btn-primary";
  add.appendChild(document.createTextNode("View on Map"));
  add.onclick = function ()
  {
    Zoom(value, floodZone);
  }
  d.appendChild(add);

  return d;
}
