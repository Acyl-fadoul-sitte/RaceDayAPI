
using System.Collections.Generic;

namespace RaceDayMVC1.Models;

public class EventCategoriesViewModel
{
    public EventViewModel Event { get; set; } = new();

    public List<CategoryViewModel> Categories { get; set; } = new();
}

