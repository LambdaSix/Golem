using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Golem.Database
{
    [Table("entity_component")]
    public class EntityComponent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("EntityId")]
        public Entity Entity { get; set; }

        [Column("entity_id")]
        public int EntityId { get; set; }

        [ForeignKey("ComponentId")]
        public Component Component { get; set; }

        [Column("component_id")]
        public int ComponentId { get; set; }
    }
}