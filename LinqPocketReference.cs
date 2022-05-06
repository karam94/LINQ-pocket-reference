using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Linq_pocket_reference
{
    public class LinqPocketReference
    {
        private string[] Push(string[] array, string newValue) => array.Append(newValue).ToArray();

        [Fact]
        public void LinqStrings()
        {
            var namesA = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" };
            var namesB = new[] { "Tom", "Dick", "Bob" };
            var asEnumerable = namesA.AsEnumerable();
            var asList = namesA.ToList();
            var asStringArray = asList.Cast<string>();
            var asArray = asList.ToArray();

            namesA.Length.Should().Be(5);
            namesA.TryGetNonEnumeratedCount(out var count);
            count.Should().Be(5);
            namesA.ElementAt(2).Should().Be("Harry");
            namesA.ElementAt(^2).Should().BeEquivalentTo("Mary");
            namesA.ElementAtOrDefault(200).Should().BeNull();
            namesA.SequenceEqual(namesB).Should().BeFalse();
            namesA.Where(x => x.Length >= 4).Should().BeEquivalentTo("Dick", "Harry", "Mary");
            namesA.Any(x => x.StartsWith("H")).Should().BeTrue();
            namesA.Contains("Harry").Should().BeTrue();
            namesA.Skip(3).Should().BeEquivalentTo("Mary", "Jay");
            namesA.Take(3).Should().BeEquivalentTo("Tom", "Dick", "Harry");
            namesA.Take(1..).Should().BeEquivalentTo("Dick", "Harry", "Mary", "Jay");
            namesA.Take(1..3).Should().BeEquivalentTo("Dick", "Harry");
            namesA.Take(^2..).Should().BeEquivalentTo("Mary", "Jay");
            namesA.First().Should().BeEquivalentTo("Tom");
            namesA.First(x => x.Length > 3).Should().BeEquivalentTo("Dick");
            namesA.Last().Should().BeEquivalentTo("Jay");
            namesA.Last(x => x.Length > 3).Should().BeEquivalentTo("Mary");
            Push(namesA, "Tom").Distinct().Should().BeEquivalentTo("Tom", "Dick", "Harry", "Mary", "Jay");
            Push(namesA, "Tom").DistinctBy(x => x.Length).Should().BeEquivalentTo("Harry", "Dick", "Tom");
            namesA.SkipWhile(x => !x.Contains('a')).Should().BeEquivalentTo("Harry", "Mary", "Jay");
            namesA.TakeWhile(x => !x.Contains('a')).Should().BeEquivalentTo("Tom", "Dick");
            namesA.Where(x => x.Length == namesA.OrderBy(y => y.Length).First().Length).Should()
                .BeEquivalentTo("Tom", "Jay");
            namesA.Where(x => x.Length == namesA.Min(y => y.Length)).Should().BeEquivalentTo("Tom", "Jay");
            namesA.OrderBy(x => x.Length).ThenBy(x => x[0]).Should()
                .BeEquivalentTo("Jay", "Tom", "Dick", "Mary", "Harry");
            namesB.Reverse().Should().BeEquivalentTo("Bob", "Dick", "Tom");
            namesA.MaxBy(x => x.Length).Should().Be("Harry");
            namesA.MinBy(x => x.Length).Should().Be("Tom");
            namesA.Chunk(2).Should().BeEquivalentTo(new[]
                { new[] { "Tom", "Dick" }, new[] { "Harry", "Mary" }, new[] { "Jay" } });

            var emptyArray = Array.Empty<string>();
            emptyArray.FirstOrDefault().Should().BeNull();
            emptyArray.FirstOrDefault("DefaultValue").Should().BeEquivalentTo("DefaultValue");
            emptyArray.LastOrDefault("DefaultValue").Should().BeEquivalentTo("DefaultValue");
            emptyArray.SingleOrDefault("DefaultValue").Should().BeEquivalentTo("DefaultValue");
            namesA.DefaultIfEmpty().Should().BeEquivalentTo(namesA);
            emptyArray.DefaultIfEmpty().Should().BeEquivalentTo(new[] { (object)null });
            emptyArray.DefaultIfEmpty("default").Should().BeEquivalentTo("default");

            Enumerable.Repeat("bananas", 3).Should().BeEquivalentTo("bananas", "bananas", "bananas");
            Enumerable.Range(1, 5).Zip(namesA, namesB).Should().BeEquivalentTo(new[]
            {
                (
                    1,
                    "Tom",
                    "Tom"
                ),
                (
                    2,
                    "Dick",
                    "Dick"
                ),
                (
                    3,
                    "Harry",
                    "Bob"
                )
            });

            // Inner Join (same as Intersect)
            namesA.Join(namesB, a => a, b => b, (a, _) => a).Should().BeEquivalentTo("Tom", "Dick");
            namesA.Union(namesB).Should().BeEquivalentTo("Tom", "Dick", "Harry", "Mary", "Jay", "Bob");
            namesA.Intersect(namesB).Should().BeEquivalentTo("Tom", "Dick");
            namesA.Except(namesB).Should().BeEquivalentTo("Harry", "Mary", "Jay");
            namesA.Concat(namesB).Should().BeEquivalentTo("Tom", "Dick", "Harry", "Mary", "Jay", "Tom", "Dick", "Bob");

            var querySyntax1 =
                from x in namesA
                where x.Contains('a')
                orderby x.Length
                select x.ToUpper();
            querySyntax1.Should().BeEquivalentTo("JAY", "MARY", "HARRY");
            namesA.Where(x => x.Contains('a'))
                .OrderBy(x => x.Length)
                .Select(x => x.ToUpper()).Should().BeEquivalentTo("JAY", "MARY", "HARRY");

            var querySyntax2 =
                from x in namesA
                where x.Length > 3
                select x
                into namesOverThreeChars
                where namesOverThreeChars.Contains('a')
                select namesOverThreeChars;
            querySyntax2.Should().BeEquivalentTo("Harry", "Mary");
            namesA.Where(x => x.Length > 3).Where(x => x.Contains('a')).Should().BeEquivalentTo("Harry", "Mary");

            var querySyntax3 =
                from x in namesA
                select x[0];
            querySyntax3.Should().BeEquivalentTo(new[] { 'T', 'D', 'M', 'H', 'J' });
            namesA.Select(x => x[0]).Should().BeEquivalentTo(new[] { 'T', 'D', 'M', 'H', 'J' });

            var querySyntax4 =
                from x in namesA
                orderby x.Length descending, x[0]
                select x;
            querySyntax4.Should().ContainInOrder("Harry", "Dick", "Mary", "Jay", "Tom");
            namesA.OrderByDescending(x => x.Length).ThenBy(x => x[0]).Should()
                .ContainInOrder("Harry", "Dick", "Mary", "Jay", "Tom");

            var querySyntax5 =
                from name in namesA
                select new
                {
                    Original = name,
                    Vowelless = Regex.Replace(name, "[aeiou]", "")
                }
                into tempName
                where tempName.Vowelless.Length > 2
                select tempName;
            querySyntax5.ToArray().Should().BeEquivalentTo(new[]
            {
                new { Original = "Dick", Vowelless = "Dck" },
                new { Original = "Harry", Vowelless = "Hrry" },
                new { Original = "Mary", Vowelless = "Mry" },
            });
            namesA.Select(x => new
                {
                    Original = x,
                    Vowelless = Regex.Replace(x, "[aeiou]", "")
                })
                .Where(x => x.Vowelless.Length > 2)
                .ToArray().Should().BeEquivalentTo(new[]
                {
                    new { Original = "Dick", Vowelless = "Dck" },
                    new { Original = "Harry", Vowelless = "Hrry" },
                    new { Original = "Mary", Vowelless = "Mry" },
                });

            namesA.Select((x, i) => new
            {
                Index = i,
                Name = x
            }).Should().BeEquivalentTo(new[]
            {
                new { Index = 0, Name = "Tom" },
                new { Index = 1, Name = "Dick" },
                new { Index = 2, Name = "Harry" },
                new { Index = 3, Name = "Mary" },
                new { Index = 4, Name = "Jay" }
            });

            namesA.Concat(namesB).GroupBy(name => name)
                .Select(grouping => new { Person = grouping.Key, Votes = grouping.Count() }).Should()
                .BeEquivalentTo(new[]
                {
                    new
                    {
                        Person = "Tom",
                        Votes = 2
                    },
                    new
                    {
                        Person = "Dick",
                        Votes = 2
                    },
                    new
                    {
                        Person = "Harry",
                        Votes = 1
                    },
                    new
                    {
                        Person = "Bob",
                        Votes = 1
                    },
                    new
                    {
                        Person = "Mary",
                        Votes = 1
                    },
                    new
                    {
                        Person = "Jay",
                        Votes = 1
                    }
                });
        }

        [Fact]
        public void LinqObjects()
        {
            var locations = new[]
            {
                new
                {
                    Name = "United Kingdom",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "Germany",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "China",
                    ContinentId = "AS"
                }
            };

            var continents = new[]
            {
                new
                {
                    Name = "Europe",
                    ContinentId = "EU",
                    Countries = new[] { "France", "Germany" }
                },
                new
                {
                    Name = "Asia",
                    ContinentId = "AS",
                    Countries = new[] { "China", "UAE" }
                }
            };

            var moreLocations = new[]
            {
                new
                {
                    Name = "United Kingdom",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "Spain",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "Japan",
                    ContinentId = "AS"
                }
            };

            continents.ToDictionary(k => k.ContinentId, v => v.Name).Should().BeEquivalentTo(
                new Dictionary<string, string>
                {
                    { "EU", "Europe" },
                    { "AS", "Asia" }
                });

            var querySyntax1 =
                from continent in continents
                select continent.Countries;
            querySyntax1.Should()
                .BeEquivalentTo(new[] { new[] { "France", "Germany" }, new[] { "China", "UAE" } });
            continents.Select(x => x.Countries).Should()
                .BeEquivalentTo(new[] { new[] { "France", "Germany" }, new[] { "China", "UAE" } });

            continents.SelectMany(x => x.Countries).Should().BeEquivalentTo("France", "Germany", "China", "UAE");

            continents.SelectMany(continent => continent.Countries.Select(country => new
            {
                CountryCode = country.Substring(0, 3).ToUpper()
            })).Should().BeEquivalentTo(new[]
            {
                new { CountryCode = "FRA" },
                new { CountryCode = "GER" },
                new { CountryCode = "CHI" },
                new { CountryCode = "UAE" }
            });

            locations.Union(moreLocations).Should().BeEquivalentTo(new[]
            {
                new
                {
                    Name = "United Kingdom",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "Germany",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "Spain",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "China",
                    ContinentId = "AS"
                },
                new
                {
                    Name = "Japan",
                    ContinentId = "AS"
                }
            });

            locations.UnionBy(moreLocations, x => x.ContinentId).Should().BeEquivalentTo(new[]
            {
                new
                {
                    Name = "United Kingdom",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "China",
                    ContinentId = "AS"
                }
            });

            locations.Intersect(moreLocations).Should().BeEquivalentTo(new[]
            {
                new
                {
                    Name = "United Kingdom",
                    ContinentId = "EU"
                }
            });

            locations.IntersectBy(moreLocations.Select(x => x.ContinentId), x => x.ContinentId).Should().BeEquivalentTo(
                new[]
                {
                    new
                    {
                        Name = "United Kingdom",
                        ContinentId = "EU"
                    },
                    new
                    {
                        Name = "China",
                        ContinentId = "AS"
                    }
                });

            locations.Except(moreLocations).Should().BeEquivalentTo(new[]
            {
                new
                {
                    Name = "Germany",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "China",
                    ContinentId = "AS"
                }
            });

            locations.ExceptBy(moreLocations.Select(x => x.Name), x => x.Name).Should().BeEquivalentTo(new[]
            {
                new
                {
                    Name = "Germany",
                    ContinentId = "EU"
                },
                new
                {
                    Name = "China",
                    ContinentId = "AS"
                }
            });

            // Cross Join
            var querySyntax2 =
                from country1 in continents[0].Countries
                from country2 in continents[1].Countries
                select country1 + " vs. " + country2;
            querySyntax2.Should()
                .BeEquivalentTo("France vs. China", "France vs. UAE", "Germany vs. China", "Germany vs. UAE");
            continents[0].Countries
                .SelectMany(_ => continents[1].Countries, (country1, country2) => country1 + " vs. " + country2)
                .Should().BeEquivalentTo("France vs. China", "France vs. UAE", "Germany vs. China", "Germany vs. UAE");

            // Inner Join
            var querySyntax3 =
                from continent in continents
                join location in locations
                    on continent.ContinentId equals location.ContinentId
                select new
                {
                    ContinentId = continent.ContinentId,
                    ContinentName = continent.Name,
                    LocationName = location.Name
                };
            querySyntax3.Should().BeEquivalentTo(new[]
            {
                new
                {
                    ContinentId = "EU",
                    ContinentName = "Europe",
                    LocationName = "United Kingdom"
                },
                new
                {
                    ContinentId = "EU",
                    ContinentName = "Europe",
                    LocationName = "Germany"
                },
                new
                {
                    ContinentId = "AS",
                    ContinentName = "Asia",
                    LocationName = "China"
                }
            });
            // E.g. SELECT L.Name, C.Name FROM Locations L INNER JOIN Continents C ON L.ContinentId = C.ContinentId 
            locations.Join(continents, location => location.ContinentId, continent => continent.ContinentId,
                (location, continent) =>
                    new
                    {
                        ContinentId = continent.ContinentId,
                        ContinentName = continent.Name,
                        LocationName = location.Name
                    }).Should().BeEquivalentTo(new[]
            {
                new
                {
                    ContinentId = "EU",
                    ContinentName = "Europe",
                    LocationName = "United Kingdom"
                },
                new
                {
                    ContinentId = "EU",
                    ContinentName = "Europe",
                    LocationName = "Germany"
                },
                new
                {
                    ContinentId = "AS",
                    ContinentName = "Asia",
                    LocationName = "China"
                }
            });

            // Inner Join (But yields a hierarchical grouped result rather than flat result
            var querySyntax4 =
                from continent in continents
                join location in locations
                    on continent.ContinentId equals location.ContinentId
                    into matchingLocations
                select new
                {
                    ContinentId = continent.ContinentId,
                    ContinentName = continent.Name,
                    LocationNames = from location in matchingLocations
                        select location.Name
                };
            querySyntax4.Should().BeEquivalentTo(new[]
            {
                new
                {
                    ContinentId = "EU",
                    ContinentName = "Europe",
                    LocationNames = new[] { "United Kingdom", "Germany" }
                },
                new
                {
                    ContinentId = "AS",
                    ContinentName = "Asia",
                    LocationNames = new[] { "China" }
                }
            });

            continents.GroupJoin(locations, continent => continent.ContinentId,
                location => location.ContinentId,
                (continent, matchingLocations) => new
                {
                    ContinentId = continent.ContinentId,
                    ContinentName = continent.Name,
                    LocationNames = matchingLocations.Select(location => location.Name).ToArray()
                }).Should().BeEquivalentTo(new[]
            {
                new
                {
                    ContinentId = "EU",
                    ContinentName = "Europe",
                    LocationNames = new[] { "United Kingdom", "Germany" }
                },
                new
                {
                    ContinentId = "AS",
                    ContinentName = "Asia",
                    LocationNames = new[] { "China" }
                }
            });
        }

        [Fact]
        public void LinqIntegers()
        {
            var numbersA = new[] { 1, 2, 3 };
            var numbersB = new[] { 3, 4, 5 };

            numbersA.Min().Should().Be(1);
            numbersA.Max().Should().Be(3);
            numbersA.Sum().Should().Be(6);
            numbersA.Average().Should().Be((1 + 2 + 3) / numbersA.Length);
            numbersA.Count().Should().Be(3);
            numbersA.All(x => x < 5).Should().BeTrue();
            numbersA.SequenceEqual(new[] { 1, 2, 3 }).Should().BeTrue();
            numbersA.GroupBy(x => (x % 2 == 0)).Should().BeEquivalentTo(new[] { new[] { 1, 3 }, new[] { 2 } });
            numbersA.Concat(numbersB).Should().BeEquivalentTo(new[] { 1, 2, 3, 3, 4, 5 });
            numbersA.Union(numbersB).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });

            Enumerable.Range(1, 5).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
            Enumerable.Repeat(1, 5).Should().BeEquivalentTo(new[] { 1, 1, 1, 1, 1 });
            // Enumerable.Range(0, Int32.MaxValue).Concat(Enumerable.Range(0, 10)).LongCount().Should().Be(2147483657);

            // Aggregate applies a function to each item of a collection
            // Like a recursive function
            // 0 is the seed (initial value)
            // Result  = (0 + 1) + 2 + 3 = 6
            numbersA.Aggregate(0, (seed, n) => seed + n).Should().Be(6);
            // Result = 
            // Iteration 1) ((0 + 1) + 2 + 3)/2.0  = 6.0/2.0 = 3
            numbersA.Aggregate(0, (seed, n) => seed + n, response => response / 2.0m).Should().Be(3);
            // Result  = (0 + 1) * 2 = 2 * 3 = 6
            // Same as 3! (3 factorial)
            numbersA.Aggregate((seed, n) => seed * n).Should().Be(6);
        }

        [Fact]
        public void LazyExecutionExample()
        {
            var numbers = new List<int> { 1 };
            numbers.Should().BeEquivalentTo(new List<int> { 1 });

            // Query can be constructed
            var query = numbers.Select(x => x * 10);
            numbers.Add(2);
            numbers.Should().BeEquivalentTo(new List<int> { 1, 2 });

            // BUT query is not run until used
            // Proof of this is that 20 is present
            // This means the query is ran on Line 45 & not Line 38
            var queryResult = query.ToList();
            queryResult.Should().BeEquivalentTo(new List<int> { 10, 20 });
        }

        [Fact]
        public void OuterVariableExample()
        {
            var numbers = new[] { 1, 2 };
            var factor = 10;

            // Created with factor 10, but not executed
            var query = numbers.Select(x => x * factor);

            // Therefore when factor is updated here, it affects the query
            factor = 20;

            // And thus, the query executes with factor 20
            query.Should().BeEquivalentTo(new[] { 20, 40 });
        }

        [Fact]
        public void WhenYouHaveIQueryable_DecideBetweenQueryingOnDbOrClient()
        {
            // Imagine this is EF
            var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" };
            IQueryable<string> dbContext = new EnumerableQuery<string>(names);

            // IQueryable PREPARED to be run on db, has not run yet
            var databaseQuery = dbContext.Where(x => x.Length > 4);

            // Runs query on db via .AsEnumerable(), .ToArray(), .ToList()
            var databaseResults = databaseQuery.ToList();
            databaseResults.Should().BeEquivalentTo("Harry");

            // Runs query in memory/on client
            var clientResults = dbContext.ToList().Where(x => x.Length > 4);
            clientResults.Should().BeEquivalentTo("Harry");

            // Tl;DR - To tweak performance, decide whether to query on DB or client
        }
    }
}