using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Alchemy;

namespace Hogs.RPG.Core.Registries
{
    public static class AlchemyPotionRegistry
    {
        public static readonly Dictionary<string, AlchemyPotionDefinition> All = new()
        {
            // Level 1
            { AlchemyPotionDefinitions.WeakStaminaVial.Id,    AlchemyPotionDefinitions.WeakStaminaVial    },
            { AlchemyPotionDefinitions.ApprenticesBrew.Id,    AlchemyPotionDefinitions.ApprenticesBrew    },

            // Level 10
            { AlchemyPotionDefinitions.WeakHuntersDraft.Id,   AlchemyPotionDefinitions.WeakHuntersDraft   },

            // Level 15
            { AlchemyPotionDefinitions.StaminaVial.Id,        AlchemyPotionDefinitions.StaminaVial        },
            { AlchemyPotionDefinitions.TrailTonic.Id,         AlchemyPotionDefinitions.TrailTonic         },

            // Level 20
            { AlchemyPotionDefinitions.HuntersDraft.Id,       AlchemyPotionDefinitions.HuntersDraft       },

            // Level 25
            { AlchemyPotionDefinitions.XpSerum.Id,            AlchemyPotionDefinitions.XpSerum            },

            // Level 30
            { AlchemyPotionDefinitions.WeakBerserkerBrew.Id,  AlchemyPotionDefinitions.WeakBerserkerBrew  },
            { AlchemyPotionDefinitions.WeakIronbloodTonic.Id, AlchemyPotionDefinitions.WeakIronbloodTonic },

            // Level 40
            { AlchemyPotionDefinitions.GoldRushFlask.Id,      AlchemyPotionDefinitions.GoldRushFlask      },

            // Level 50
            { AlchemyPotionDefinitions.BerserkerBrew.Id,      AlchemyPotionDefinitions.BerserkerBrew      },
            { AlchemyPotionDefinitions.IronbloodTonic.Id,     AlchemyPotionDefinitions.IronbloodTonic     },

            // Level 60
            { AlchemyPotionDefinitions.Antivenom.Id,          AlchemyPotionDefinitions.Antivenom          },
            { AlchemyPotionDefinitions.BlacksmithsElixir.Id,  AlchemyPotionDefinitions.BlacksmithsElixir  },

            // Level 70
            { AlchemyPotionDefinitions.SwiftfootBrew.Id,      AlchemyPotionDefinitions.SwiftfootBrew      },
            { AlchemyPotionDefinitions.RaidElixir.Id,         AlchemyPotionDefinitions.RaidElixir         },

            // Level 80
            { AlchemyPotionDefinitions.BloodPactPotion.Id,    AlchemyPotionDefinitions.BloodPactPotion    },
            { AlchemyPotionDefinitions.GreaterStaminaVial.Id, AlchemyPotionDefinitions.GreaterStaminaVial },

            // Level 85
            { AlchemyPotionDefinitions.RevivalDraught.Id,     AlchemyPotionDefinitions.RevivalDraught     },

            // Level 90
            { AlchemyPotionDefinitions.GreaterHuntersDraft.Id,AlchemyPotionDefinitions.GreaterHuntersDraft},
            { AlchemyPotionDefinitions.GreaterXpSerum.Id,     AlchemyPotionDefinitions.GreaterXpSerum     },

            // Level 95
            { AlchemyPotionDefinitions.ShadowSalve.Id,        AlchemyPotionDefinitions.ShadowSalve        },

            // Level 99
            { AlchemyPotionDefinitions.VoidTincture.Id,       AlchemyPotionDefinitions.VoidTincture       },
            { AlchemyPotionDefinitions.DragonBlood.Id,        AlchemyPotionDefinitions.DragonBlood        },
        };
    }
}