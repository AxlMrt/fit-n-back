using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using System.Linq.Expressions;

namespace FitnessApp.Modules.Exercises.Domain.Specifications
{
    public abstract class Specification<T>
    {
        public abstract Expression<Func<T, bool>> ToExpression();

        public Func<T, bool> ToFunc() => ToExpression().Compile();

        public bool IsSatisfiedBy(T entity) => ToFunc()(entity);

        public Specification<T> And(Specification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        public Specification<T> Or(Specification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        public Specification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }

    internal class AndSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpr = _left.ToExpression();
            var rightExpr = _right.ToExpression();

            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                new ParameterReplacer(parameter).Visit(leftExpr.Body)!,
                new ParameterReplacer(parameter).Visit(rightExpr.Body)!);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

    internal class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpr = _left.ToExpression();
            var rightExpr = _right.ToExpression();

            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.OrElse(
                new ParameterReplacer(parameter).Visit(leftExpr.Body)!,
                new ParameterReplacer(parameter).Visit(rightExpr.Body)!);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

    internal class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _inner;

        public NotSpecification(Specification<T> inner)
        {
            _inner = inner;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var innerExpr = _inner.ToExpression();
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.Not(new ParameterReplacer(parameter).Visit(innerExpr.Body)!);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

    internal class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }

    // Exercise-specific specifications
    public class ExerciseByNameSpecification : Specification<Exercise>
    {
        private readonly string _name;

        public ExerciseByNameSpecification(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override Expression<Func<Exercise, bool>> ToExpression()
        {
            return exercise => exercise.Name.ToLower().Contains(_name.ToLower());
        }
    }

    public class ExerciseByTypeSpecification : Specification<Exercise>
    {
        private readonly ExerciseType _type;

        public ExerciseByTypeSpecification(ExerciseType type)
        {
            _type = type;
        }

        public override Expression<Func<Exercise, bool>> ToExpression()
        {
            return exercise => exercise.Type == _type;
        }
    }

    public class ExerciseByDifficultySpecification : Specification<Exercise>
    {
        private readonly DifficultyLevel _difficulty;

        public ExerciseByDifficultySpecification(DifficultyLevel difficulty)
        {
            _difficulty = difficulty;
        }

        public override Expression<Func<Exercise, bool>> ToExpression()
        {
            return exercise => exercise.Difficulty == _difficulty;
        }
    }

    public class ExerciseByMuscleGroupSpecification : Specification<Exercise>
    {
        private readonly MuscleGroup _muscleGroup;

        public ExerciseByMuscleGroupSpecification(MuscleGroup muscleGroup)
        {
            _muscleGroup = muscleGroup;
        }

        public override Expression<Func<Exercise, bool>> ToExpression()
        {
            return exercise => exercise.MuscleGroups.HasFlag(_muscleGroup);
        }
    }

    public class ExerciseRequiresEquipmentSpecification : Specification<Exercise>
    {
        private readonly bool _requiresEquipment;

        public ExerciseRequiresEquipmentSpecification(bool requiresEquipment)
        {
            _requiresEquipment = requiresEquipment;
        }

        public override Expression<Func<Exercise, bool>> ToExpression()
        {
            if (_requiresEquipment)
                return exercise => exercise.Equipment.Items.Any();
            else
                return exercise => !exercise.Equipment.Items.Any();
        }
    }

    public class ActiveExerciseSpecification : Specification<Exercise>
    {
        public override Expression<Func<Exercise, bool>> ToExpression()
        {
            return exercise => exercise.IsActive;
        }
    }
}
