# ReusableCodes
Reuseable codes in my coding life.  
If you need it, you just need add comment before the code:
```
//Code From: https://github.com/Flithor/ReusableCodes/{file path}
```

**Disclaimer: I am not responsible for any consequences of anyone trusting and using my code.**

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
