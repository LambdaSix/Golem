using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Golem.Database
{
    [Table("component")]
    public class Component
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("ComponentTypeId")]
        public ComponentType ComponentType { get; set; }

        [Column("component_type_id")]
        public int ComponentTypeId { get; set; }

        [Column("component_data")]
        public string ComponentData { get; set; }
    }
}