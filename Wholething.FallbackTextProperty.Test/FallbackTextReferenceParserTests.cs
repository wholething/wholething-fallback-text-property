using Wholething.FallbackTextProperty.Services;
using Wholething.FallbackTextProperty.Services.Impl;
using Xunit;

namespace Wholething.FallbackTextProperty.Test
{
    public class FallbackTextReferenceParserTests
    {
        private readonly IFallbackTextReferenceParser _referenceParser;

        public FallbackTextReferenceParserTests()
        {
            _referenceParser = new FallbackTextReferenceParser();
        }
        
        [Fact]
        public void InvalidTemplate()
        {
            var template = "This is a {{1234:test}} template";
            var references = _referenceParser.Parse(template);

            Assert.Empty(references);
        }

        [Fact]
        public void ValidTemplate()
        {
            var template = "This is a {{ancestor(blogPost):companyName}} template This is a {{ancestor(1, 3, 4):companyAddress}} template";
            var references = _referenceParser.Parse(template);

            Assert.Equal(2, references.Count);

            Assert.Equal("ancestor", references[0].Function);
            Assert.Single(references[0].Args);
            Assert.Equal("blogPost", references[0].Args[0]);

            Assert.Equal("ancestor", references[1].Function);
            Assert.Equal(3, references[1].Args.Length);
            Assert.Equal("1", references[1].Args[0]);
        }
    }
}
