void LayoutTest_OnDetailInitialize()
{
    CarCheckDateTime.Value = DateTime.Now;
    LungCheckDateTime.Value = DateTime.Now;
    
    var data = Los.AddRow();
    data.No.Value = "4ABC123";
    data.Type.Value = "Ford Mustang";
    data = Los.AddRow();
    data.No.Value = "7XYZ890";
    data.Type.Value = "Chevrolet Camaro";
    data = Los.AddRow();
    data.No.Value = "1JKL234";
    data.Type.Value = "Tesla Model S";
    data = Los.AddRow();
    data.No.Value = "5MNO567";
    data.Type.Value = "Dodge Charger";
    data = Los.AddRow();
    data.No.Value = "3PQR678";
    data.Type.Value = "Toyota Camry";
    
    data = Chicago.AddRow();
    data.No.Value = "1LMN012";
    data.Type.Value = "Mercedes-Benz C-Class";
    data = Chicago.AddRow();
    data.No.Value = "2OPQ345";
    data.Type.Value = "Audi A4";
    data = Chicago.AddRow();
    data.No.Value = "3RST567";
    data.Type.Value = "Lexus RX";
    data = Chicago.AddRow();
    data.No.Value = "4UVW890";
    data.Type.Value = "Volkswagen Jetta";
    data = Chicago.AddRow();
    data.No.Value = "5XYZ123";
    data.Type.Value = "Subaru Outback";
    data = Chicago.AddRow();
    data.No.Value = "6ABC456";
    data.Type.Value = "Nissan Altima";

}

void ToLos_OnClick()
{
   foreach(var row in Chicago.Rows)
   {
        if (row.Check.Value)
        {
            Chicago.DeleteRow(row);
            Los.AddRow(row);
        }
   }
}

void ToChicago_OnClick()
{
   foreach(var row in Los.Rows)
   {
        if (row.Check.Value)
        {
            Los.DeleteRow(row);
            Chicago.AddRow(row);
        }
   }
}