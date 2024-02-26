using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Bug", 
    "S1848:Objects should not be created to be dropped immediately without being used", 
    Justification = "Constructs add themselves to the scope in which they are created")]
[assembly: SuppressMessage("Performance", "CA1806:Do not ignore method results", 
    Justification = "Constructs add themselves to the scope in which they are created")]
[assembly: SuppressMessage("Potential Code Quality Issues", 
    "RECS0026:Possible unassigned object created by 'new'", 
    Justification = "Constructs add themselves to the scope in which they are created")]

