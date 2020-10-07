namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System.Collections.Generic;
    using AutoFixture;
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal;
    using SqlStreamStore.Streams;
    using Xunit;

    public class When_initializing_processed_stream_state_with_no_runner_position
    {
        private readonly ProcessedStreamState _sut;

        public When_initializing_processed_stream_state_with_no_runner_position()
            => _sut = new ProcessedStreamState(null);

        [Fact]
        public void Then_the_position_should_be_minus_one()
            => _sut.Position.Should().Be(-1L);

        [Fact]
        public void Then_the_last_processed_message_position_should_be_empty()
            => _sut.LastProcessedMessagePosition.Should().BeNull();

        [Fact]
        public void Then_the_expected_next_position_should_be_zero()
            => _sut.ExpectedNextPosition.Should().Be(0L);

        [Fact]
        public void Then_state_should_not_be_changed()
            => _sut.HasChanged.Should().BeFalse();
    }

    public class When_initializing_processed_stream_state_with_a_negative_runner_position
    {
        private readonly ProcessedStreamState _sut;

        public When_initializing_processed_stream_state_with_a_negative_runner_position()
        {
            var position = new Fixture().CreateNegative<long>();
            _sut = new ProcessedStreamState(position);
        }

        [Fact]
        public void Then_the_position_should_be_minus_one()
            => _sut.Position.Should().Be(-1L);

        [Fact]
        public void Then_the_last_processed_message_position_should_be_empty()
            => _sut.LastProcessedMessagePosition.Should().BeNull();

        [Fact]
        public void Then_the_expected_next_position_should_be_zero()
            => _sut.ExpectedNextPosition.Should().Be(0L);

        [Fact]
        public void Then_state_should_not_be_changed()
            => _sut.HasChanged.Should().BeFalse();
    }

    public class When_updating_the_processed_state_with_a_message
    {
        private readonly ProcessedStreamState _sut;

        public When_updating_the_processed_state_with_a_message()
        {
            var fixture = new Fixture();
            var position = fixture.Create<long>();
            _sut = new ProcessedStreamState(position);

            _sut.UpdateWithProcessed(fixture.Create<StreamMessage>());
        }

        [Fact]
        public void Then_the_last_processed_message_position_should_not_be_empty()
            => _sut.LastProcessedMessagePosition.Should().NotBeNull();
    }

    public class When_updating_the_processed_state_with_a_message_with_the_same_position_as_the_runner
    {
        private readonly ProcessedStreamState _sut;
        private readonly long _runnerPosition;

        public When_updating_the_processed_state_with_a_message_with_the_same_position_as_the_runner()
        {
            var fixture = new Fixture();
            _runnerPosition = fixture.CreatePositive<long>();
            StreamMessage message = fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(_runnerPosition);

            _sut = new ProcessedStreamState(_runnerPosition);
            _sut.UpdateWithProcessed(message);
        }

        [Fact]
        public void Then_the_position_should_be_the_runner_position()
            => _sut.Position.Should().Be(_runnerPosition);

        [Fact]
        public void Then_the_last_processed_message_position_should_be_the_runner_position()
            => _sut.LastProcessedMessagePosition.Should().Be(_runnerPosition);

        [Fact]
        public void Then_the_expected_next_position_should_be_the_runner_position_plus_one()
            => _sut.ExpectedNextPosition.Should().Be(_runnerPosition + 1);

        [Fact]
        public void Then_state_should_not_be_changed()
            => _sut.HasChanged.Should().BeFalse();
    }

    public class When_updating_the_processed_state_with_a_message_with_position_before_the_runner_position
    {
        private readonly ProcessedStreamState _sut;
        private readonly long _runnerPosition;
        private readonly StreamMessage _message;

        public When_updating_the_processed_state_with_a_message_with_position_before_the_runner_position()
        {
            var fixture = new Fixture();
            _runnerPosition = fixture
                .CreatePositive<long>()
                .WithMinimumValueOf(3);

            _message = fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(_runnerPosition
                    .CreateRandomLowerValue()
                    .WithMinimumValueOf(0));

            _sut = new ProcessedStreamState(_runnerPosition);
            _sut.UpdateWithProcessed(_message);
        }

        [Fact]
        public void Then_the_position_should_be_the_runner_position()
            => _sut.Position.Should().Be(_runnerPosition);

        [Fact]
        public void Then_the_last_processed_message_position_should_be_the_message_position()
            => _sut.LastProcessedMessagePosition.Should().Be(_message.Position);

        [Fact]
        public void Then_the_expected_next_position_should_be_the_runner_position_plus_one()
            => _sut.ExpectedNextPosition.Should().Be(_runnerPosition + 1);

        [Fact]
        public void Then_state_should_not_be_changed()
            => _sut.HasChanged.Should().BeFalse();
    }

    public class When_updating_the_processed_state_with_the_expected_next_message
    {
        private readonly ProcessedStreamState _sut;
        private readonly long _runnerPosition;
        private readonly StreamMessage _message;

        public When_updating_the_processed_state_with_the_expected_next_message()
        {
            var fixture = new Fixture();
            _runnerPosition = fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 1);

            _message = fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(_runnerPosition + 1);

            _sut = new ProcessedStreamState(_runnerPosition);
            _sut.UpdateWithProcessed(_message);
        }

        [Fact]
        public void Then_the_position_should_be_the_message_position()
            => _sut.Position.Should().Be(_message.Position);

        [Fact]
        public void Then_the_last_processed_message_position_should_be_the_message_position()
            => _sut.LastProcessedMessagePosition.Should().Be(_message.Position);

        [Fact]
        public void Then_the_expected_next_position_should_be_the_message_position_plus_one()
            => _sut.ExpectedNextPosition.Should().Be(_message.Position + 1);

        [Fact]
        public void Then_state_should_not_be_changed()
            => _sut.HasChanged.Should().BeTrue();
    }

    public class When_updating_the_processed_state_with_a_message_skipping_some_positions
    {
        private readonly ProcessedStreamState _sut;
        private readonly long _runnerPosition;
        private readonly StreamMessage _message;

        public When_updating_the_processed_state_with_a_message_skipping_some_positions()
        {
            var fixture = new Fixture();
            _runnerPosition =  fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue -2);

            _message = fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(_runnerPosition.CreateRandomHigherValue());

            _sut = new ProcessedStreamState(_runnerPosition);
            _sut.UpdateWithProcessed(_message);
        }

        [Fact]
        public void Then_the_position_should_be_the_message_position()
            => _sut.Position.Should().Be(_message.Position);

        [Fact]
        public void Then_the_last_processed_message_position_should_be_the_message_position()
            => _sut.LastProcessedMessagePosition.Should().Be(_message.Position);

        [Fact]
        public void Then_the_expected_next_position_should_be_the_message_position_plus_one()
            => _sut.ExpectedNextPosition.Should().Be(_message.Position + 1);

        [Fact]
        public void Then_state_should_not_be_changed()
            => _sut.HasChanged.Should().BeTrue();
    }

    public class When_determining_the_gap_positions_for_a_state_with_a_message_with_position_before_the_runner_position
    {
        private readonly ProcessedStreamState _sut;
        private readonly Fixture _fixture;

        public When_determining_the_gap_positions_for_a_state_with_a_message_with_position_before_the_runner_position()
        {
            _fixture = new Fixture();
            var runnerPosition = _fixture
                .CreatePositive<long>()
                .WithMinimumValueOf(5);
            
            _sut = new ProcessedStreamState(runnerPosition);
        }

        [Fact]
        public void Then_the_gap_positions_should_be_empty()
        {
            var position = _sut
                .Position
                .CreateRandomLowerValue();

            var message = _fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(position);

            _sut.DetermineGapPositions(message).Should().BeEmpty();
        }
    }

    public class When_determining_the_gap_positions_for_a_state_with_the_next_expected_message
    {
        private readonly ProcessedStreamState _sut;
        private readonly Fixture _fixture;

        public When_determining_the_gap_positions_for_a_state_with_the_next_expected_message()
        {
            _fixture = new Fixture();
            var runnerPosition = _fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 1);

            _sut = new ProcessedStreamState(runnerPosition);
        }

        [Fact]
        public void Then_the_gap_positions_should_be_empty()
        {
            var message = _fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(_sut.ExpectedNextPosition);

            _sut.DetermineGapPositions(message).Should().BeEmpty();
        }
    }

    public class When_determining_the_gap_positions_for_a_state_with_a_message_skipping_some_positions
    {
        private readonly ProcessedStreamState _sut;
        private readonly Fixture _fixture;

        public When_determining_the_gap_positions_for_a_state_with_a_message_skipping_some_positions()
        {
            _fixture = new Fixture();
            var runnerPosition = _fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 2);

            _sut = new ProcessedStreamState(runnerPosition);
        }

        [Fact]
        public void Then_the_gap_positions_should_from_expected_to_current_message_minus_one()
        {
            StreamMessage message = _fixture
                .Create<ConfigurableStreamMessage>()
                .WithPosition(_sut.ExpectedNextPosition.CreateRandomHigherValue());

            var expectedGapPositions = new List<long>();
            for (var i = _sut.ExpectedNextPosition; i < message.Position; i++)
                expectedGapPositions.Add(i);

            _sut.DetermineGapPositions(message)
                .Should()
                .Equal(expectedGapPositions)
                .And.BeInAscendingOrder();
        }
    }
}
