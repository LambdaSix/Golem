using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Golem.Database
{
    public class ComponentType
    {
        [Key][DatabaseGenerated(DatabaseGeneratedOption.None)][Column("id")]
        public int Id { get; set; }

        [Column("component_type")]
        public int Type { get; set; }
    }
}