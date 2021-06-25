using System;
using System.Collections.Generic;
using System.Linq;

namespace powerplant.Core
{
    public class ProductionPlanComputer
    {
        /**
	 * Updates (if possible) the responseEs array to balance the energy produced
	 *  in order to allow a new powerplant with excessive pmin to be used.
	 * If possible, balances it and returns true
	 * If not possible, returns false and doesnt apply any changes.
	 * This function should NOT be called multiple times because it means it should have been called with a lower "remainingToReduce" first.
	 */
        // I dont like recursivity, because of obvious callstack length, speed and memory consuption reasons,
        //  especially in intensely computing applications
        private static bool FallbackReducePower(
            IReadOnlyList<PowerplantEfficiency> powerplants,
            List<ResponseE> responseEs,
            int curIndex,
            int remainingToReduce)
        {
            // If the first powerplant is too powerful, it's not possible to balance the power.
            // Returning false will skip the powerplant.
            if (curIndex == 0) return false;

            // Dry run, check if it's possible to remove that much energy from previous powerplants

            // Amount of power we can remove from previous engines
            var canBeRemoved = 0;

            // Loop-exiting condition:
            var enoughCanBeRemoved = false;

            // For each loop starting with the least efficient one (highest index), try to remove as much
            //  power as we can, to see if we can exceed the value of remainingToReduce
            for (var i = curIndex-1; i > 0; --i)
            {
                // shorthand
                var pe = powerplants[i];

                // We have to use responseEs and not powerplants because (for example in windmills, because of the
                //  wind%) the pmax is not always the current p
                canBeRemoved += (responseEs[i].p /* current power */ - pe.p.Pmin /* min power */);

                // ReSharper disable once InvertIf - for clarity
                if (canBeRemoved > remainingToReduce)
                {
                    enoughCanBeRemoved = true;
                    break;
                }
            }

            // If the first loop shown that not enough power can be removed, false must be returned prematurely.
            if (!enoughCanBeRemoved) return false;

            // The power can be removed, apply the removal.
            for (var i = curIndex - 1; i > 0; --i)
            {
                // shorthands
                var pe = powerplants[i];
                var saveable = responseEs[i].p - pe.p.Pmin;

                if (remainingToReduce <= 0) break;
            
                if (remainingToReduce >= saveable)
                {
                    responseEs[i].p = pe.p.Pmin;
                    remainingToReduce -= saveable;
                }
                else /*if (remainingToReduce < saveable)*/
                {
                    responseEs[i].p -= remainingToReduce;
                    // No need to update remainingToReduce (1. it's 0 and 2. we early return)
                    return true;
                }
            }

            return true;
        }

        private static bool FallbackIncreasePower(IReadOnlyList<PowerplantEfficiency> powerplants,
            List<ResponseE> responseEs,
            int remainingToAdd,
            Fuels rFuels)
        {
            // No need for dry run this time, just increase it as much as we can.
            // That function is fairly straightforward.
            // Both lists are sorted the same order

            for (var i = 0; i != responseEs.Count; ++i)
            {

                // Shorthands for clarity
                var plant = powerplants[i];
                var resp = responseEs[i];
                var maxProducedPower = plant.p.PMaxForFuels(rFuels);
                var curProducedPower = resp.p;

                if (curProducedPower == maxProducedPower) continue;

                var available = maxProducedPower - curProducedPower;

                // Should not happen but just in case
                if (remainingToAdd == 0) return true;

                // The quantity of energy available in this powerplant is not
                //  sufficient to complete the missing part, so make it go full throttle
                if (remainingToAdd > available)
                {
                    responseEs[i].p += available;
                    remainingToAdd -= available;
                }

                // The quantity of energy is sufficient in this powerplant to complete
                //  the system lack, so adjust it and return
                if (remainingToAdd < available)
                {
                    responseEs[i].p += remainingToAdd;
                    return true;
                }

            }

            // There is no way to add more energy to the system
            return false;
        }

        public static List<ResponseE> Compute(Request r, out bool success)
        {
            // The amount of energy we still need
            var wanted = r.load;

            // Array of powerplant and their efficiency, used in case green energy is not enough.
            // Usage described in step 2 comment
            var efficiencies = new List<PowerplantEfficiency>();

            // returned value
            var ret = new List<ResponseE>();

            // Check if green electricity is enough to power 
            var greenElec = r.Powerplants.Where(p => p.IsGreenEnergy()).Sum(o => o.PMaxForFuels(r.fuels));

            // STEP 1
            // Implementation in case we want to distribute evenly between windmill powerplants (in order to limit noise)
            // It's only useful if green electricity is enough, else we will just run them at full power in step 2 because they are the most efficient
            // If we want to start as few powerplants as we can, just remove that block of code and comment out the .Where() below the block
            //  to make a list of all powerplants. (instead of all non-green-energy powerplants).
            if(wanted <= greenElec)
            {
                // Yay enough green electricity !

                var percent = (float)wanted / greenElec;
                r.Powerplants.ToList().ForEach(p =>
                {
                    var responseE = new ResponseE(p.name);

                    if (p.IsGreenEnergy())
                        responseE.p = (int)Math.Ceiling(p.Pmax * percent); // Ensure it's rounded up: worst case +1 MWH total to avoid the worst case: -1 MWH total
                    else
                        responseE.p = 0; // Not wind turbine, oil is baaad.

                    ret.Add(responseE);
                });

                success = true;
                return ret;
            }

            // Green energy is not enough, still add them in the output
            r.Powerplants
                .Where(p => p.IsGreenEnergy()).ToList()
                .ForEach(p => ret.Add(new ResponseE(p.name, p.PMaxForFuels(r.fuels))));

            // Remove the green-produced energy from the needed pool.
            wanted -= greenElec;

            // STEP 2
            // Populate the "efficiencies" list so we can have a sorted-by-efficiency list of powerplants.
            // We then try to set the most efficient to their maximum and lower the use of the less efficient ones.
            // (we know they are the most efficient AND not sufficient, so no need to check anything to add them, so we skip all the checks
            //  we have to do with the other powerplants)
            r.Powerplants.ToList().ForEach(p => efficiencies.Add(new PowerplantEfficiency(p, r.fuels)));
            efficiencies.Sort();

            // Since the green powerplants are at the beginning of the loop, and since they have already been added above
            // We skip them by starting the loop at their number
            var greenPowerplantCount = r.Powerplants.Count(powerplant => powerplant.IsGreenEnergy());


            for (var i = greenPowerplantCount; i != efficiencies.Count(); ++i)
            {
                // Ignore green energy powerplants, since they have already been manually added. They should all be at the beginning tho.
                if (efficiencies[i].p.IsGreenEnergy()) continue;

                var eff = efficiencies[i];

                // If we still have energy missing
                if (wanted > 0)
                {
                    var addFromThis = Math.Min(wanted, eff.p.Pmax);

                    // If we require only 50MWH and the pmin of this powerplant is 100MWH (for example)
                    // We have to either fallback to reduce the power of previous ones (if possible)
                    // Or we have to skip this powerplant and try to compensate with the next ones. (I dont like this idea
                    //  because the next ones will be less efficient, thus more expensive to use)
                    if(wanted < eff.p.Pmin)
                    {
                        // Try to remove some power from the previous powerplants, especially the ones the less efficient
                        // Since they are sorted by efficiency, the higher the index is, the less efficient is the powerplant
                        if (FallbackReducePower(efficiencies, ret, i, eff.p.Pmin - wanted))
                        {
                            addFromThis = eff.p.Pmin;

                            // Previous ones have been adjusted, so we have exactly the power we want. But it's done at the end of the loop, just kept here for clarity.
                            // wanted = 0;
                        }
                        else
                        {
                            // If this powerplant is too powerful, and we cant lower the power of previous ones, skip this powerplant and try to get the
                            //  next one to provide enough power.
                            addFromThis = 0;
                        }
                    }

                    ret.Add(new ResponseE(eff.p.name, addFromThis));
                    wanted -= addFromThis;
                }
                else
                    // If we already matched the required energy
                {
                    ret.Add(new ResponseE(eff.p.name, 0));
                }			
            }

            // If there's energy missing
            if (wanted > 0 && !FallbackIncreasePower(efficiencies, ret, wanted, r.fuels))
            {
                success = false; 
                return ret;
            }

            success = true;
            return ret;
        }
    }
}
