# ReusableCodes
Reuseable codes in my coding life.  
If you need it, you just need add comment before the code:
```
//Code From: https://github.com/Flithor/ReusableCodes/{file path}
```

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

#### DropDownControl
[DropDownControl.xaml](https://github.com/Flithor/ReusableCodes/blob/main/WPF/DropDownControl.xaml)  
[DropDownControl.cs](https://github.com/Flithor/ReusableCodes/blob/main/WPF/DropDownControl.cs)  
A control with ComboBox like but can contains any custom content.

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

## Program common
#### [SingletonHelper.cs](https://github.com/Flithor/ReusableCodes/blob/main/Program/SingletonHelper.cs)
An easy to use helper class to help you check your program has any other running instance exists.
