# ReusableCodes
Reuseable codes in my coding life.  
If you need it, you just need add comment before the code:
```
//Code From: https://github.com/Flithor/ReusableCodes/{file path}
```

**Disclaimer:  
I am not responsible for any consequences of anyone trusting and using my code.  
Treat any code you got by CTRL+C/V with caution.**

## AppSetting
#### [AppSetting.cs](https://github.com/Flithor/ReusableCodes/blob/main/AppSetting/AppSettings.cs)
Read and instant save configuration.

## Debug
#### [ConsoleTextBox.cs](https://github.com/Flithor/ReusableCodes/blob/main/Debug/ConsoleTextBox.cs)
A TextBox Control(WPF) redirect console output, contains a method to show a simple window with it.

## NetWork
#### [TcpLinker.cs](https://github.com/Flithor/ReusableCodes/blob/main/Network/TcpLinker.cs)
Use TcpListener accept TCP connection requests and receive data.
#### [TcpLinker_MultiPort.cs](https://github.com/Flithor/ReusableCodes/blob/main/Network/TcpLinker_MultiPort.cs)
Use TcpListener accept TCP connection requests and receive data, open multiple port.

## WPF
#### [Slider.Style.NumericUpDown.xaml](https://github.com/Flithor/ReusableCodes/blob/main/WPF/Slider.Style.NumericUpDown.xaml)
A MAGIC to turn WPF native control `Slider` to `NumericUpDown`.
Learn More here: https://stackoverflow.com/a/63734191/6859121

#### [AssemblyVersion.cs](https://github.com/Flithor/ReusableCodes/blob/main/WPF/AssemblyVersion.cs)
A `MarkupExtension` work in xaml for output the file version of current or target type assembly.  
Example: `<Window Title="{mk:AssemblyVersion Formatter='My Software V{0}'}">`

#### DropDownControl
Default style: [DropDownControl.xaml](https://github.com/Flithor/ReusableCodes/blob/main/WPF/DropDownControl.xaml)  
Control codes: [DropDownControl.cs](https://github.com/Flithor/ReusableCodes/blob/main/WPF/DropDownControl.cs)  
A Custom Control with ComboBox like but can contains any custom content.

#### FormField
Default style: [FormField.xaml](https://github.com/Flithor/ReusableCodes/blob/main/WPF/FormField.xaml)  
Control codes: [FormField.cs](https://github.com/Flithor/ReusableCodes/blob/main/WPF/FormField.cs)  
A control that lets you quickly layout your forms grid.  
Example:
```
<Grid ctrl:FormField.Gap="5">
  <Grid.ColumnDefinitions>
    <ColumnDefinition />
    <ColumnDefinition />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions>
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <ctrl:FormField Header="ID"><TextBox Text="{Binding ID}"/></ctrl:FormField>
  <ctrl:FormField Header="Name" Column="1"><TextBox Text="{Binding Name}"/></ctrl:FormField>
  <ctrl:FormField Header="Age" Row="1"><TextBox Text="{Binding Age}"/></ctrl:FormField>
  <ctrl:FormField Header="Gender" Row="1" Column="1"><ComboBox SelectedItem="{Binding Gender}" ItemsSource="{x:Static loc:Common.Genders}"/></ctrl:FormField>
</Grid>
```

## [Avalonia UI](https://github.com/AvaloniaUI/Avalonia)
#### FormField
Default style: [FormField.xaml](https://github.com/Flithor/ReusableCodes/blob/main/AvaloniaUI/FormField.axaml)  
Control codes: [FormField.cs](https://github.com/Flithor/ReusableCodes/blob/main/AvaloniaUI/FormField.axaml.cs)  
A control that lets you quickly layout your forms grid.  
Example Same on WPF version.

## EF Core
#### [BulkInsert.cs](https://github.com/Flithor/ReusableCodes/blob/main/EFCore/BulkInsert.cs)
An efficient batch insert expansion method with EF functional style **for MySQL**  
**Attention**: It is not perfect, for example: it does not consider \[Column] and other ways to specify the mapping column name.

#### [OrPredicate.cs](https://github.com/Flithor/ReusableCodes/blob/main/EFCore/OrPredicate.cs)
A linq way extension make IQueryable<T> support or predicate.  
Example:
```
// IQueryable<ClassA> myQuery = ....;
  
var queryOr = myQuery.AsWhereOr();
// for a condition list ...
// queryOr = queryOr.WhereOr(a => /*some condition*/)

myQuery = queryOr.AsQueryable();
```
**Attention**: `Where` will be an independent condition, it will "and" with `WhereOr`.  
**Attention 2**: It is not implemented in accordance with IQueryable paradigm, but its usage is almost the same as linq, and it works well.

#### [EFRemoveExtension.cs](https://github.com/Flithor/ReusableCodes/blob/main/EFCore/EFRemoveExtension.cs)
Some extension methods to make you remove data by condition or get primary keys to use them in some other place.  
It's optimized to only query the primary key unless you need the entity.  
Example:
```
// Delete by match item
dbContext.RemoveWhere<YourEntity>(e => e.name == deleteName && e.type == deleteType));
dbContext.SaveChanges();

// Delete by id list
dbContext.RemoveWhere<YourEntity>(e => ids.Contains(e.id));
dbContext.SaveChanges();

//Fit for DbSet<T>
dbContext.Set<YourEntity>().RemoveWhere(e => e.name = deleteName));
dbContext.SaveChanges();

//Get deleted entities
var deletedItems = dbContext.RemoveWhereAndTake<YourEntity>(e => ids.Contains(e.id));
dbContext.SaveChanges();
return deletedItems;

//Get primary keys of deleted items, and use them
var deletedKeys = dbContext.RemoveWhereAndTakeKeys<YourEntity>(e => e.name.Contains(deleteKeyWord)).Select(e => e.id).ToList();
dbContext.RemoveWhere<RelatedEntity>(re => deletedKeys.Contains(re.parent_key));
dbContext.SaveChanges();
```

## Program common
#### [SingletonHelper.cs](https://github.com/Flithor/ReusableCodes/blob/main/Program/SingletonHelper.cs)
An easy to use helper class to help you check your program has any other running instance exists.

#### [PublishDllIntoFolder](https://github.com/Flithor/ReusableCodes/blob/main/Program/PublishDllIntoFolder)
A **BLACK MAGIC** from [dnspy](https://github.com/dnSpy/dnSpy) to publish all referenced dlls into "bin" folder, make a clean and refreshing publish folder.
1. Copy files in [Properties](https://github.com/Flithor/ReusableCodes/tree/main/Program/PublishDllIntoFolder/Properties) to your project's `Properties` folder.
2. Edit your project file(`.csproj`) according to [ExampleProject](https://github.com/Flithor/ReusableCodes/blob/main/Program/PublishDllIntoFolder/ExampleProject.csproj).
   - Pay attention to read the comments!
3. Try it!

PS: you can edit `DllIntoFolder.targets` to change the dll folder name. `bin` by default.

**WARNING**:
It will affect relative path file seek based on working directory.  
If you need to use this, please be sure to handle possible path errors in your code.
